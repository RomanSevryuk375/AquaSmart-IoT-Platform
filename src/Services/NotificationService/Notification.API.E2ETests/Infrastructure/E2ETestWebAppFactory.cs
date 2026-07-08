using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.Extensions;
using Notification.Infrastructure.IntegrationTests.Infrastructure;

namespace Notification.API.E2ETests.Infrastructure;

public sealed class E2ETestWebAppFactory : IntegrationTestWebAppFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            ServiceDescriptor? userContextDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IUserContext));
            if (userContextDescriptor != null)
            {
                services.Remove(userContextDescriptor);
            }

            services.AddScoped<IUserContext, UserContext>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            services.AddMassTransitTestHarness();
        });
    }
}
