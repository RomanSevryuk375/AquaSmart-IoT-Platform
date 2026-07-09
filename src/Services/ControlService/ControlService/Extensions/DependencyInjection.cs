using Control.Application.Extensions;
using Control.Infrastructure.Extensions;
using Microsoft.OpenApi.Models;

namespace Control.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMySwagger();
        services.AddCommonAuthentication(configuration);
        services.AddAquaAuthorizationPolicies();
        services.AddControllers();
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    public static IServiceCollection AddMySwagger(this IServiceCollection services)
    {
        return services.AddEndpointsApiExplorer()
                       .AddSwaggerGen(options =>
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
