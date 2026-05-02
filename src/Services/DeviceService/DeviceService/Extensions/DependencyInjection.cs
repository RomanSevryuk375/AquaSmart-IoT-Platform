using Contracts.Authorization;
using Device.Application.Extesions;
using Device.Infrastructure.Extensions;
using Microsoft.OpenApi.Models;

namespace Device.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter a valid JWT access token."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });

        services.AddCommonAuthentication(configuration);
        services.AddControllers();

        services.AddServices();
        services.AddRepositories(configuration);
        services.AddQuartzJobs();
        services.AddRabbitMq(configuration);

        services.AddAquaAuthorizationPolicies();

        return services;
    }
}
