using BuildingBlocks.TestShared;
using Control.Domain.Interfaces;
using NSubstitute;
using BuildingBlocks.Domain.Results;

namespace Control.Infrastructure.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : BaseIntegrationTestWebAppFactory<Program, ControlDbContext>
{
    protected override string GetDbConnectionStringName() => "ConnectionStrings:ControlDbContext";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            IHardwareValidator hardwareValidatorMock = Substitute.For<IHardwareValidator>();
            hardwareValidatorMock.ValidateAssignmentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(Result.Success());
            services.AddSingleton(hardwareValidatorMock);
        });
    }
}
