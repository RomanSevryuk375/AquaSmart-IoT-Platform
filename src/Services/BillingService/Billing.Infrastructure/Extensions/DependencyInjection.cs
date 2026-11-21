using Billing.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Billing.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddPostgresDbContext<BillingDbContext>(configuration)
                .AddDapper<BillingDbContext>();
}
