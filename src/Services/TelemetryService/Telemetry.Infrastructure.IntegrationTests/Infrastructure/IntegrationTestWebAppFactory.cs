using BuildingBlocks.Domain.Results;
using BuildingBlocks.TestShared;
using MassTransit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Infrastructure.Persistence;
using Telemetry.TestShared.Constants;

namespace Telemetry.Infrastructure.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : BaseIntegrationTestWebAppFactory<Program, TelemetryDbContext>
{
    protected override string GetDbConnectionStringName() => "ConnectionStrings:TelemetryDbContext";

    protected override void ConfigureMassTransit(IServiceCollection services)
    {
        services.AddMassTransitTestHarness(x => x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); }));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.AddScoped<BuildingBlocks.Infrastructure.Data.Outbox.OutboxMessageProcessorService<TelemetryDbContext>>();

            services.PostConfigure<HealthCheckServiceOptions>(options =>
            {
                var uniqueRegistrations = options.Registrations
                    .GroupBy(r => r.Name)
                    .Select(g => g.First())
                    .ToList();
                options.Registrations.Clear();
                foreach (HealthCheckRegistration? reg in uniqueRegistrations)
                {
                    options.Registrations.Add(reg);
                }
            });

            services.AddSingleton(Substitute.For<ITelemetryNotifier>());

            IDeviceTokenValidator tokenValidatorMock = Substitute.For<IDeviceTokenValidator>();
            tokenValidatorMock.ValidateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto { ControllerId = TestConstants.ControllerId, UserId = TestConstants.UserId }));
            services.AddSingleton(tokenValidatorMock);
        });
    }
}
