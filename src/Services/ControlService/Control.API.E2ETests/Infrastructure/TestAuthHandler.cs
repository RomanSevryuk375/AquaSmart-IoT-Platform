using System.Security.Claims;
using System.Text.Encodings.Web;
using Contracts.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Control.API.E2ETests.Infrastructure;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.Authorization.ToString().Contains("Test"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, ControlTestConstants.UserId.ToString()),
            new Claim(CustomClaims.Permissions, SubPermissions.TankRead),
            new Claim(CustomClaims.Permissions, SubPermissions.TankCreate),
            new Claim(CustomClaims.Permissions, SubPermissions.TankUpdate),
            new Claim(CustomClaims.Permissions, SubPermissions.TankDelete),
            new Claim(CustomClaims.Permissions, SubPermissions.AutoRuleCreate),
            new Claim(CustomClaims.Permissions, SubPermissions.AutoScheduleCreate),
            new Claim(CustomClaims.Permissions, SubPermissions.VacationMode)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
