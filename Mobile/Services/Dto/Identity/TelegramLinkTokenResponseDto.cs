using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Identity;

public sealed record TelegramLinkTokenResponseDto(
    [property: JsonPropertyName("link")] string Link,
    [property: JsonPropertyName("expireAt")] DateTime ExpireAt
);
