using BuildingBlocks.Infrastructure.Extensions;
using Device.API.Extensions;
using Device.API.gRPC;
using Microsoft.AspNetCore.Server.Kestrel.Core;

await MicroserviceRunner.RunAsync("AquaSmart.DeviceService", args, builder =>
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(50051, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
    });

    builder.Services.AddConfiguration(builder.Configuration);
}, app =>
{
    app.MapGrpcService<DeviceIntegrationEndpoint>();
});

public partial class Program
{
    protected Program() { }
}
