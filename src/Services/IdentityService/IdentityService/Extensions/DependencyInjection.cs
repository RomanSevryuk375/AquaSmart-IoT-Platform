using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.Extensions;
using IdentityService.API.Endpoints.Auth;
using IdentityService.Application.Extensions;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

        services.AddGlobalApi(configuration);
        services.AddScoped<ICookieHelpers, CookieHelpers>();
        
        services.AddEndpointsApiExplorer();
        services.AddEndpoints(typeof(DependencyInjection).Assembly);
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);

        return services;
    }
}
