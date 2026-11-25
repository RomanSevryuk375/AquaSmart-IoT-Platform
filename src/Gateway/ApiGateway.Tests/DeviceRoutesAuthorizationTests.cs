using System.Net;
using ApiGateway.Tests.Infrastructure;
using BuildingBlocks.Presentation.Constants;
using FluentAssertions;

namespace ApiGateway.Tests;

[Collection("GatewayTests")]
public class DeviceRoutesAuthorizationTests(GatewayFactoriesFixture fixture)
{
    private const string SampleDeviceToken = "ak_test_device_token_12345";
    private const string SampleMacAddress = "AA:BB:CC:DD:EE:FF";

    public static IEnumerable<object[]> DeviceRouteCases()
    {
        string controllerId = "11111111-1111-1111-1111-111111111111";
        string commandId = "22222222-2222-2222-2222-222222222222";
        string campaignId = "33333333-3333-3333-3333-333333333333";

        foreach (string env in new[] { "Development", "Production" })
        {
            yield return [env, HttpMethod.Get, "/api/device/v1/controllers/me/config", "device-api"];
            yield return [env, HttpMethod.Post, $"/api/device/v1/controllers/{controllerId}/ping", "device-api"];
            yield return [env, HttpMethod.Get, $"/api/device/v1/commands/pending/{controllerId}", "device-api"];
            yield return [env, HttpMethod.Post, $"/api/device/v1/commands/{commandId}/complete", "device-api"];
            yield return [env, HttpMethod.Post, $"/api/device/v1/commands/{commandId}/fail", "device-api"];
            yield return [env, HttpMethod.Post, "/api/telemetry/v1/data", "telemetry-api"];
            yield return [env, HttpMethod.Get, $"/api/firmware/v1/ota/check?controller_id={controllerId}", "firmware-api"];
            yield return [env, HttpMethod.Get, $"/api/firmware/v1/ota/download/{campaignId}?controller_id={controllerId}", "firmware-api"];
            yield return [env, HttpMethod.Post, "/api/firmware/v1/ota/status", "firmware-api"];
        }
    }

    [Theory]
    [MemberData(nameof(DeviceRouteCases))]
    public async Task DeviceRoutes_WithoutJwt_AreForwardedWithDeviceHeaders(
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
        request.Headers.Add(ApiConstants.Headers.DeviceToken, SampleDeviceToken);
        request.Headers.Add(ApiConstants.Headers.MacAddress, SampleMacAddress);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ForwardedRequests.Should().ContainSingle();

        GatewayTestWebAppFactory.RecordedDownstreamRequest forwarded = factory.ForwardedRequests.Single();
        forwarded.Method.Should().Be(method);
        forwarded.RequestUri.Should().NotBeNull();
        forwarded.RequestUri!.Host.Should().Be(expectedClusterHost);
        forwarded.RequestUri.PathAndQuery.Should().Be(pathAndQuery);
        forwarded.Headers.Should().ContainKey(ApiConstants.Headers.DeviceToken)
            .WhoseValue.Should().ContainSingle().Which.Should().Be(SampleDeviceToken);
        forwarded.Headers.Should().ContainKey(ApiConstants.Headers.MacAddress)
            .WhoseValue.Should().ContainSingle().Which.Should().Be(SampleMacAddress);
    }
}
