using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Domain.Results.Converters;

public sealed class ResultJsonConverter : JsonConverter<Result>
{
    public override Result? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token for Result.");
        }

        bool? isSuccess = null;
        Error? error = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propName = reader.GetString();
                reader.Read();

                if (string.Equals(propName, "isSuccess", StringComparison.OrdinalIgnoreCase))
                {
                    isSuccess = reader.GetBoolean();
                }
                else if (string.Equals(propName, "error", StringComparison.OrdinalIgnoreCase))
                {
                    error = JsonSerializer.Deserialize<Error>(ref reader, options);
                }
                else
                {
                    reader.Skip();
                }
            }
        }

        if (!isSuccess.HasValue)
        {
            throw new JsonException("Missing required 'isSuccess' property.");
        }

        return isSuccess.Value
            ? Result.Success()
            : Result.Failure(error ?? Error.Failure("Unknown", "Deserialized failure without details."));
    }

    public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isSuccess", value.IsSuccess);
        writer.WritePropertyName("error");
        JsonSerializer.Serialize(writer, value.Error, options);
        writer.WriteEndObject();
    }
}
