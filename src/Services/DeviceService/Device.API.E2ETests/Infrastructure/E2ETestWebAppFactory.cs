using Device.Infrastructure.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Device.API.E2ETests.Infrastructure;

public sealed class E2ETestWebAppFactory : IntegrationTestWebAppFactory
{
    public const string TestWebhookSecret = "test-mqtt-webhook-secret-12345";
    public const string TestWorkerUsername = "device-service-worker";
    public const string TestWorkerPassword = "test-mqtt-worker-password-12345";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("MqttBrokerOptions:WebhookSecret", TestWebhookSecret);
        builder.UseSetting("MqttOptions:Username", TestWorkerUsername);
        builder.UseSetting("MqttOptions:Password", TestWorkerPassword);

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });
    }
}
