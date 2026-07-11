using Contracts.Middlewares;
using Control.API.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ControlService application");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    builder.AddElkLogging();
    builder.Services.AddConfiguration(builder.Configuration);

    WebApplication app = builder.Build();
    app.UseGlobalExceptionHandler();
    app.AddConfiguration();

    await app.RunAsync();
}
#pragma warning disable S2139
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "ControlService terminated unexpectedly");
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
