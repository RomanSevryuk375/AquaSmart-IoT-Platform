using Device.Application.Extesions;
using Device.Infrastructure.Extensions;
using Microsoft.OpenApi.Models;

namespace Device.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddMySwaggerGen();

        services.AddCommonAuthentication(configuration);
        services.AddAquaAuthorizationPolicies();
        services.AddControllers();
        services.AddGrpc();
        services.AddInfrastructure(configuration);
        services.AddApplication(configuration);

        return services;
    }

    public static IServiceCollection AddMySwaggerGen(this IServiceCollection services)
    {
        return services.AddSwaggerGen(options =>
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
    }
}
