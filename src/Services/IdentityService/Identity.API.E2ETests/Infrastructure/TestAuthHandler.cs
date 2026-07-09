using System.Security.Claims;
using System.Text.Encodings.Web;
using Contracts.Authorization;
using Identity.Infrastructure.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.API.E2ETests.Infrastructure;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TestUserContext userContext) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.Authorization.ToString().Contains("Test"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userContext.UserId.ToString()),
            new Claim(CustomClaims.Permissions, SubPermissions.AccountView),
            new Claim(CustomClaims.Permissions, SubPermissions.AccountUpdate)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
