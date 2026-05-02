using Contracts.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Contracts.Authorization;

public static class Extensions
{
    public const string AccessTokenCookieName = "AccessToken";
    public const string RefreshTokenCookieName = "RefreshToken";

    public static IServiceCollection AddAquaAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(SubPermissions.TankRead, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.TankRead));
            })
            .AddPolicy(SubPermissions.TankCreate, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.TankCreate));
            })
            .AddPolicy(SubPermissions.TankUpdate, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.TankUpdate));
            })
            .AddPolicy(SubPermissions.TankDelete, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.TankDelete));
            })
            .AddPolicy(SubPermissions.DeviceControl, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.DeviceControl));
            })
            .AddPolicy(SubPermissions.DeviceEditManual, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.DeviceEditManual));
            })
            .AddPolicy(SubPermissions.AutoRuleCreate, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.AutoRuleCreate));
            })
            .AddPolicy(SubPermissions.AutoScheduleCreate, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.AutoScheduleCreate));
            })
            .AddPolicy(SubPermissions.VacationMode, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.VacationMode));
            })
            .AddPolicy(SubPermissions.TelemetryView, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.TelemetryView));
            })
            .AddPolicy(SubPermissions.AnalyticsHistory, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.AnalyticsHistory));
            })
            .AddPolicy(SubPermissions.DiagnosticsFull, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.DiagnosticsFull));
            })
            .AddPolicy(SubPermissions.DataRealtime, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.DataRealtime));
            })
            .AddPolicy(SubPermissions.MaintenanceLogRead, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.MaintenanceLogRead));
            })
            .AddPolicy(SubPermissions.MaintenanceLogWrite, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.MaintenanceLogWrite));
            })
            .AddPolicy(SubPermissions.ReminderManage, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.ReminderManage));
            })
            .AddPolicy(SubPermissions.EmailAlerts, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.EmailAlerts));
            })
            .AddPolicy(SubPermissions.TelegramAlerts, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.TelegramAlerts));
            })
            .AddPolicy(SubPermissions.AccountView, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.AccountView));
            })
            .AddPolicy(SubPermissions.AccountUpdate, policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(CustomClaims.Permissions, SubPermissions.AccountUpdate));
            });

        return services;
    }

    public static IServiceCollection AddCommonAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        if (jwtOptions is null || string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
        {
            throw new InvalidOperationException("JWT configuration missing or invalid.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                ConfigureJwtBearer(options, jwtOptions);
            });

        return services;
    }

    public static void ConfigureJwtBearer(JwtBearerOptions options, JwtOptions jwtOptions)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token) &&
                    context.Request.Cookies.TryGetValue(AccessTokenCookieName, out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    }
}
