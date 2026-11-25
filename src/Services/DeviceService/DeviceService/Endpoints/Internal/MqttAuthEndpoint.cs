using System.Text.Json.Serialization;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Endpoints;
using Device.API.Filters;
using Device.Application.Features.Mqtt;
using MediatR;

namespace Device.API.Endpoints.Internal;

public sealed class MqttAuthEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("internal/mqtt/auth", async (
            [FromBody] MqttAuthRequest request,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var query = new AuthenticateMqttClientQuery
            {
                Username = request.Username,
                Password = request.Password,
                ClientId = request.ClientId
            };

            Result<MqttAuthResult> result = await sender.Send(query, cancellationToken);
            if (result.IsFailure)
            {
                return Results.Ok(new MqttAuthResponse { Result = "deny", IsSuperuser = false });
            }

            return Results.Ok(new MqttAuthResponse
            {
                Result = result.Value.Allow ? "allow" : "deny",
                IsSuperuser = result.Value.IsSuperuser
            });
        })
        .WithTags("Internal MQTT")
        .Produces<MqttAuthResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous()
        .AddEndpointFilter<InternalApiKeyFilter>();
    }

    public sealed record MqttAuthRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; init; } = string.Empty;

        [JsonPropertyName("clientid")]
        public string ClientId { get; init; } = string.Empty;
    }

    public sealed record MqttAuthResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; init; } = "deny";

        [JsonPropertyName("is_superuser")]
        public bool IsSuperuser { get; init; }
    }
}
