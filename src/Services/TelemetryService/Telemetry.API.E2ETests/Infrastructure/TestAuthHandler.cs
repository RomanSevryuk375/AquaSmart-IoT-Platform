using System.Security.Claims;
using System.Text.Encodings.Web;
using BuildingBlocks.Presentation.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telemetry.TestShared.Constants;

namespace Telemetry.API.E2ETests.Infrastructure;

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
            new Claim(ClaimTypes.NameIdentifier, TestConstants.UserId.ToString()),
            new Claim(CustomClaims.Permissions, SubPermissions.DataRealtime),
            new Claim(CustomClaims.Permissions, SubPermissions.AnalyticsHistory)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
