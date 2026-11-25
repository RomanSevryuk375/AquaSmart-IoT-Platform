using ApiGateway.Extensions;
using BuildingBlocks.Infrastructure.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

const string AppName = "AquaSmart.ApiGateway";
try
{
    DotNetEnvExtensions.LoadDotNetEnv();
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.AddGlobalElkLogging(AppName);
    builder.Services.AddConfiguration(builder.Configuration);

    WebApplication app = builder.Build();

    app.AddGatewayConfiguration();

    await app.RunAsync();
}
#pragma warning disable S2139
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "{Name} terminated unexpectedly", AppName);
    throw;
}
#pragma warning restore S2139
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program
{
    protected Program() { }
}
