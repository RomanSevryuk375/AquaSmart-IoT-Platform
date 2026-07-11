// Ignore Spelling: Grpc

using Contracts.gRPC.Devices;
using Control.Application.Extensions;
using Control.Infrastructure.Extensions;
using Microsoft.OpenApi.Models;

namespace Control.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMySwaggerGen();
        services.AddCommonAuthentication(configuration);
        services.AddAquaAuthorizationPolicies();
        services.AddControllers();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddMyGrpcClient(configuration);

        return services;
    }

    public static IServiceCollection AddMyGrpcClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpcClient<DeviceIntegrationGrpc.DeviceIntegrationGrpcClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcConfiguration:DeviceServiceUrl"]!);
        });

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
