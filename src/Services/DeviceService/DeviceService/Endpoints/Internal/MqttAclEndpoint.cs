using System.Text.Json.Serialization;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Endpoints;
using Device.API.Filters;
using Device.Application.Features.Mqtt;
using MediatR;

namespace Device.API.Endpoints.Internal;

public sealed class MqttAclEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("internal/mqtt/acl", async (
            [FromBody] MqttAclRequest request,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var query = new AuthorizeMqttActionQuery
            {
                Username = request.Username,
                Topic = request.Topic,
                Action = request.Action
            };

            Result<MqttAclResult> result = await sender.Send(query, cancellationToken);
            bool allow = result.IsSuccess && result.Value.Allow;

            return Results.Ok(new MqttAclResponse
            {
                Result = allow ? "allow" : "deny"
            });
        })
        .WithTags("Internal MQTT")
        .Produces<MqttAclResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous()
        .AddEndpointFilter<InternalApiKeyFilter>();
    }

    public sealed record MqttAclRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = string.Empty;

        [JsonPropertyName("topic")]
        public string Topic { get; init; } = string.Empty;

        [JsonPropertyName("action")]
        public string Action { get; init; } = string.Empty;
    }

    public sealed record MqttAclResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; init; } = "deny";
    }
}
