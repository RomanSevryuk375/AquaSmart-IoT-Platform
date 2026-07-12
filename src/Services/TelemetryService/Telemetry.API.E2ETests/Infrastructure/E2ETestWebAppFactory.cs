using Contracts.Results;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Infrastructure.IntegrationTests.Infrastructure;
using Telemetry.TestShared.Constants;

namespace Telemetry.API.E2ETests.Infrastructure;

public sealed class E2ETestWebAppFactory : IntegrationTestWebAppFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            services.AddMassTransitTestHarness();

            IDeviceTokenValidator tokenValidatorMock = Substitute.For<IDeviceTokenValidator>();
            tokenValidatorMock.ValidateAsync(
                    TestConstants.ValidMacAddress,
                    TestConstants.ValidDeviceToken,
                    Arg.Any<CancellationToken>())
                .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto
                {
                    ControllerId = TestConstants.ControllerId,
                    UserId = TestConstants.UserId
                }));

            ServiceDescriptor? descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDeviceTokenValidator));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            services.AddSingleton<IDeviceTokenValidator>(tokenValidatorMock);
        });
    }
}
