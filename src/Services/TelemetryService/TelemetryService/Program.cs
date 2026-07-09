using Contracts.Constants;
using Contracts.Middlewares;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Telemetry.API.Extensions;
using Telemetry.Infrastructure.Persistence;
using Telemetry.Infrastructure.SignalR;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting TelemetryService application");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.AddElkLogging();

    builder.Services.AddIConfiguration(builder.Configuration);

    WebApplication app = builder.Build();

    app.UseGlobalExceptionHandler();

#pragma warning disable S6966
    using (IServiceScope scope = app.Services.CreateScope())
    {
        TelemetryDbContext context = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
        context.Database.Migrate();
    }
#pragma warning restore S6966

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks(ApiConstants.HealthRoute);
    app.MapControllers();

    app.MapHub<TelemetryHub>(SignalRRoutes.RawTelemetry);

    await app.RunAsync();
}
#pragma warning disable S2139
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "TelemetryService terminated unexpectedly");
    throw;
}
#pragma warning restore S2139
finally
{
#pragma warning disable S6966 
    Log.CloseAndFlush();
#pragma warning restore S6966 
}

#pragma warning disable S1118 
public partial class Program { }
#pragma warning restore S1118
