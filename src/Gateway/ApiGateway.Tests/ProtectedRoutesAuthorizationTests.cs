using System.Net;
using System.Net.Http.Headers;
using ApiGateway.Tests.Infrastructure;
using FluentAssertions;

namespace ApiGateway.Tests;

[Collection("GatewayTests")]
public class ProtectedRoutesAuthorizationTests(GatewayFactoriesFixture fixture)
{
    public static IEnumerable<object[]> ProtectedRouteCases()
    {
        string relayId = "11111111-1111-1111-1111-111111111111";
        string campaignId = "22222222-2222-2222-2222-222222222222";

        foreach (string env in new[] { "Development", "Production" })
        {
            yield return [env, HttpMethod.Get, "/api/device/v1/controllers", "device-api"];
            yield return [env, HttpMethod.Post, "/api/device/v1/relays", "device-api"];
            yield return [env, HttpMethod.Post, $"/api/device/v1/commands/toggle-state/{relayId}", "device-api"];
            yield return [env, HttpMethod.Get, "/api/telemetry/v1/data/raw", "telemetry-api"];
            yield return [env, HttpMethod.Get, "/api/telemetry/v1/data/aggregate", "telemetry-api"];
            yield return [env, HttpMethod.Get, "/api/telemetry/v1/data", "telemetry-api"];
            yield return [env, HttpMethod.Get, "/api/control/v1/ecosystems", "control-api"];
            yield return [env, HttpMethod.Get, "/api/notification/v1/notifications", "notification-api"];
            yield return [env, HttpMethod.Post, "/api/firmware/v1/hardware-profiles", "firmware-api"];
            yield return [env, HttpMethod.Post, "/api/firmware/v1/firmwares", "firmware-api"];
            yield return [env, HttpMethod.Post, "/api/firmware/v1/campaigns", "firmware-api"];
            yield return [env, HttpMethod.Post, $"/api/firmware/v1/campaigns/{campaignId}/start", "firmware-api"];
        }
    }

    [Theory]
    [MemberData(nameof(ProtectedRouteCases))]
    public async Task ProtectedRoutes_WithoutJwt_Return401UnauthorizedAndDoNotForward(
        string environmentName,
        HttpMethod method,
        string pathAndQuery,
        string _)
    {
        // Arrange
        GatewayTestWebAppFactory factory = fixture.GetFactory(environmentName);
        using HttpClient client = factory.CreateClient();
        factory.ClearForwardedRequests();

        using var request = new HttpRequestMessage(method, pathAndQuery);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ForwardedRequests.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(ProtectedRouteCases))]
    public async Task ProtectedRoutes_WithValidJwt_AreForwardedToExpectedCluster(
        string environmentName,
        HttpMethod method,
        string pathAndQuery,
        string expectedClusterHost)
    {
        // Arrange
        GatewayTestWebAppFactory factory = fixture.GetFactory(environmentName);
        using HttpClient client = factory.CreateClient();
        factory.ClearForwardedRequests();

        using var request = new HttpRequestMessage(method, pathAndQuery);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateSignedJwt());

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ForwardedRequests.Should().ContainSingle();
        factory.ForwardedRequests.Single().RequestUri!.Host.Should().Be(expectedClusterHost);
    }

    [Theory]
    [InlineData("Development", "/internal/mqtt/auth")]
    [InlineData("Development", "/internal/mqtt/acl")]
    [InlineData("Production", "/internal/mqtt/auth")]
    [InlineData("Production", "/internal/mqtt/acl")]
    public async Task InternalRoutes_AreNotProxiedAndReturn404NotFound(
        string environmentName,
        string path)
    {
        // Arrange
        GatewayTestWebAppFactory factory = fixture.GetFactory(environmentName);
        using HttpClient client = factory.CreateClient();
        factory.ClearForwardedRequests();

        using var unauthenticatedRequest = new HttpRequestMessage(HttpMethod.Post, path);
        using var authenticatedRequest = new HttpRequestMessage(HttpMethod.Post, path);
        authenticatedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateSignedJwt());

        // Act
        HttpResponseMessage unauthenticatedResponse = await client.SendAsync(unauthenticatedRequest);
        HttpResponseMessage authenticatedResponse = await client.SendAsync(authenticatedRequest);

        // Assert
        unauthenticatedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        authenticatedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        factory.ForwardedRequests.Should().BeEmpty();
    }
}
