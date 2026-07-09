using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Telemetry.API.Extensions;

public static class HostBuilder
{
    public static WebApplicationBuilder AddElkLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
           .ReadFrom.Configuration(context.Configuration)
           .Enrich.WithExceptionDetails()
           .WriteTo.Elasticsearch(
               new ElasticsearchSinkOptions(
               new Uri(context.Configuration["ElasticConfiguration:Uri"]!))
               {
                   IndexFormat = $"aquasmart-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                   AutoRegisterTemplate = true,
               }),
           preserveStaticLogger: true);

        return builder;
    }
}
