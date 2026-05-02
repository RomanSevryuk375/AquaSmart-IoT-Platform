using Contracts.Authorization;
using Microsoft.OpenApi.Models;
using Telemetry.Application.Extensions;
using Telemetry.Infrastructure.Extensions;

namespace Telemetry.API.Extensions;

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
        services.AddControllers();

        services.AddCommonAuthentication(configuration);
        services.AddServices();

        services.AddRepositories(configuration);
        services.AddQuartzJob();
        services.AddRabbitMq(configuration);

        services.AddAquaAuthorizationPolicies();

        return services;
    }
}
