using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Forwarder;

namespace ApiGateway.Tests.Infrastructure;

public sealed class GatewayTestWebAppFactory(string environmentName) : WebApplicationFactory<Program>
{
    public const string TestJwtIssuer = "AquaSmart.Identity";
    public const string TestJwtAudience = "AquaSmart.Gateway";

    private readonly string _jwtSecretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    private readonly RecordingDownstreamHandler _downstreamHandler = new();

    public IReadOnlyCollection<RecordedDownstreamRequest> ForwardedRequests =>
        _downstreamHandler.ForwardedRequests;

    public void ClearForwardedRequests() => _downstreamHandler.Clear();

    public string CreateSignedJwt()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestJwtIssuer,
            audience: TestJwtAudience,
            claims: [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);

        builder.UseSetting("JwtOptions:SecretKey", _jwtSecretKey);
        builder.UseSetting("JwtOptions:Issuer", TestJwtIssuer);
        builder.UseSetting("JwtOptions:Audience", TestJwtAudience);
        builder.UseSetting("JwtOptions:ExpiresHours", "12");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtOptions:SecretKey"] = _jwtSecretKey,
                ["JwtOptions:Issuer"] = TestJwtIssuer,
                ["JwtOptions:Audience"] = TestJwtAudience,
                ["JwtOptions:ExpiresHours"] = "12",
                ["Serilog:Using:1"] = "Serilog.Sinks.Console",
                ["Serilog:WriteTo:1:Name"] = "Console"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IForwarderHttpClientFactory>();
            services.AddSingleton<IForwarderHttpClientFactory>(
                new MockForwarderHttpClientFactory(_downstreamHandler));
        });
    }

    public sealed record RecordedDownstreamRequest(
        HttpMethod Method,
        Uri? RequestUri,
        Dictionary<string, string[]> Headers);

    private sealed class MockForwarderHttpClientFactory(HttpMessageHandler handler)
        : IForwarderHttpClientFactory
    {
        public HttpMessageInvoker CreateClient(ForwarderHttpClientContext context)
        {
            return new HttpMessageInvoker(handler, disposeHandler: false);
        }
    }

    private sealed class RecordingDownstreamHandler : HttpMessageHandler
    {
        private readonly ConcurrentQueue<RecordedDownstreamRequest> _requests = new();

        public IReadOnlyCollection<RecordedDownstreamRequest> ForwardedRequests =>
            _requests.Where(r => r.RequestUri?.AbsolutePath != "/health").ToArray();

        public void Clear() => _requests.Clear();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(
                h => h.Key,
                h => h.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase);

            _requests.Enqueue(new RecordedDownstreamRequest(
                request.Method,
                request.RequestUri,
                headers));

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
