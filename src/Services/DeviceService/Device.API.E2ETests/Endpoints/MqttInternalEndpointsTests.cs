using System.Text.Json.Serialization;
using BuildingBlocks.Presentation.Constants;
using Device.Application.Extesions;
using Device.Domain.Entities;

namespace Device.API.E2ETests.Endpoints;

public class MqttInternalEndpointsTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    public async Task Auth_WithoutInternalApiKey_Returns401Unauthorized()
    {
        // Arrange
        using HttpClient client = Factory.CreateClient();
        var payload = new
        {
            username = "AA:BB:CC:DD:EE:FF",
            password = TestConstants.ValidRawToken,
            clientid = "aqua-esp32-AABBCCDDEEFF"
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/internal/mqtt/auth", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Auth_WithInvalidInternalApiKey_Returns401Unauthorized()
    {
        // Arrange
        using HttpClient client = Factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/mqtt/auth")
        {
            Content = JsonContent.Create(new
            {
                username = "AA:BB:CC:DD:EE:FF",
                password = TestConstants.ValidRawToken,
                clientid = "aqua-esp32-AABBCCDDEEFF"
            })
        };
        request.Headers.Add(ApiConstants.Headers.InternalApiKey, "wrong-secret");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Acl_WithoutInternalApiKey_Returns401Unauthorized()
    {
        // Arrange
        using HttpClient client = Factory.CreateClient();
        var payload = new
        {
            username = "AA:BB:CC:DD:EE:FF",
            topic = "aquasmart/devices/AABBCCDDEEFF/telemetry",
            action = "publish"
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/internal/mqtt/acl", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Auth_WithValidControllerAndSecret_ReturnsAllowNonSuperuser()
    {
        // Arrange
        var hasher = new MyHasher();
        Controller controller = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithMacAddress("AA:BB:CC:DD:EE:11")
            .WithDeviceTokenHash(hasher.Generate(TestConstants.ValidRawToken))
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        using HttpClient client = Factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/mqtt/auth")
        {
            Content = JsonContent.Create(new
            {
                username = "aabbccddee11",
                password = TestConstants.ValidRawToken,
                clientid = "aqua-esp32-AABBCCDDEE11"
            })
        };
        request.Headers.Add(ApiConstants.Headers.InternalApiKey, E2ETestWebAppFactory.TestWebhookSecret);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AuthResponseDto? body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.Should().NotBeNull();
        body!.Result.Should().Be("allow");
        body.IsSuperuser.Should().BeFalse();
    }

    [Fact]
    public async Task Auth_WithWorkerCredentials_ReturnsAllowSuperuser()
    {
        // Arrange
        using HttpClient client = Factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/mqtt/auth")
        {
            Content = JsonContent.Create(new
            {
                username = E2ETestWebAppFactory.TestWorkerUsername,
                password = E2ETestWebAppFactory.TestWorkerPassword,
                clientid = "aqua-device-service-worker"
            })
        };
        request.Headers.Add(ApiConstants.Headers.InternalApiKey, E2ETestWebAppFactory.TestWebhookSecret);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AuthResponseDto? body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.Should().NotBeNull();
        body!.Result.Should().Be("allow");
        body.IsSuperuser.Should().BeTrue();
    }

    [Fact]
    public async Task Auth_WithInvalidDeviceToken_ReturnsDeny()
    {
        // Arrange
        var hasher = new MyHasher();
        Controller controller = new ControllerBuilder()
            .WithId(Guid.NewGuid())
            .WithMacAddress("AA:BB:CC:DD:EE:22")
            .WithDeviceTokenHash(hasher.Generate(TestConstants.ValidRawToken))
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        using HttpClient client = Factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/mqtt/auth")
        {
            Content = JsonContent.Create(new
            {
                username = "AA:BB:CC:DD:EE:22",
                password = "invalid-token",
                clientid = "aqua-esp32-AABBCCDDEE22"
            })
        };
        request.Headers.Add(ApiConstants.Headers.InternalApiKey, E2ETestWebAppFactory.TestWebhookSecret);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AuthResponseDto? body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.Should().NotBeNull();
        body!.Result.Should().Be("deny");
        body.IsSuperuser.Should().BeFalse();
    }

    [Theory]
    [InlineData("AA:BB:CC:DD:EE:FF", "aquasmart/devices/AABBCCDDEEFF/telemetry", "publish", "allow")]
    [InlineData("AA:BB:CC:DD:EE:FF", "aquasmart/devices/AABBCCDDEEFF/commands", "subscribe", "allow")]
    [InlineData("AA:BB:CC:DD:EE:FF", "aquasmart/devices/112233445566/telemetry", "publish", "deny")]
    [InlineData("AA:BB:CC:DD:EE:FF", "#", "subscribe", "deny")]
    public async Task Acl_WithValidInternalApiKey_EvaluatesTopicAccess(
        string username,
        string topic,
        string action,
        string expectedResult)
    {
        // Arrange
        using HttpClient client = Factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/mqtt/acl")
        {
            Content = JsonContent.Create(new { username, topic, action })
        };
        request.Headers.Add(ApiConstants.Headers.InternalApiKey, E2ETestWebAppFactory.TestWebhookSecret);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AclResponseDto? body = await response.Content.ReadFromJsonAsync<AclResponseDto>();
        body.Should().NotBeNull();
        body!.Result.Should().Be(expectedResult);
    }

    private sealed record AuthResponseDto
    {
        [JsonPropertyName("result")]
        public string Result { get; init; } = string.Empty;

        [JsonPropertyName("is_superuser")]
        public bool IsSuperuser { get; init; }
    }

    private sealed record AclResponseDto
    {
        [JsonPropertyName("result")]
        public string Result { get; init; } = string.Empty;
    }
}
