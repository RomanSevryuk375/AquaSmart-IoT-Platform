using BuildingBlocks.Presentation.Authorization;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Presentation.Extensions;

public static class GlobalPresentationExtensions
{
    public static IServiceCollection AddGlobalApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGlobalExceptionHandler();
        services.AddMySwaggerGen();
        services.AddCommonAuthentication(configuration);
        services.AddAquaAuthorizationPolicies();

        return services;
    }

    public static WebApplication AddGlobalConfiguration(this WebApplication application)
    {
        application.UseGlobalExceptionHandler();
        application.Use((context, next) =>
        {
            context.Request.EnableBuffering();
            return next();
        });
        application.UseSwagger();
        application.UseSwaggerUI();
        application.UseAuthentication();
        application.UseAuthorization();
        application.MapHealthChecks(ApiConstants.HealthRoute);
        application.MapEndpoints();

        return application;
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
