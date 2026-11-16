using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Identity;

public record UserProfileResponseDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("phoneNumber")] string? PhoneNumber,
    [property: JsonPropertyName("subscriptionId")] Guid SubscriptionId,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);
