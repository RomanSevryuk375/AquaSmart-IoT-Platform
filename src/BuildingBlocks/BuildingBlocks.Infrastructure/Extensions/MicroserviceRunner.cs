// Ignore Spelling: Microservice

using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class MicroserviceRunner
{
    public static async Task RunAsync(
        string appName,
        string[] args,
        Action<WebApplicationBuilder> configureServices,
        Action<WebApplication>? configureApp = null)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting {Name}", appName);

            DotNetEnvExtensions.LoadDotNetEnv();
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.AddGlobalElkLogging(appName);
            configureServices(builder);

            WebApplication app = builder.Build();

            app.AddGlobalConfiguration();

            configureApp?.Invoke(app);

            await app.RunAsync();
        }
#pragma warning disable S2139 
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            Log.Fatal(ex, "{Name} terminated unexpectedly", appName);
            throw;
        }
#pragma warning restore S2139
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
