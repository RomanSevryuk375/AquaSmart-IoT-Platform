using Contracts.Constants;
using Contracts.Middlewares;
using IdentityService.API.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting IdentityService application");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.AddElkLogging();

    builder.Services.AddConfiguration(builder.Configuration);

    WebApplication app = builder.Build();

    app.UseGlobalExceptionHandler();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealthChecks(ApiConstants.HealthRoute);
    app.MapControllers();

    await app.RunAsync();
}
#pragma warning disable S2139
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "IdentityService terminated unexpectedly");
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
