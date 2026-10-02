using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Domain.Results.Converters;

public sealed class ResultOfTJsonConverter<T> : JsonConverter<Result<T>>
{
    public override Result<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token for Result<T>.");
        }

        bool? isSuccess = null;
        T? value = default;
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
                else if (string.Equals(propName, "value", StringComparison.OrdinalIgnoreCase))
                {
                    value = JsonSerializer.Deserialize<T>(ref reader, options);
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
            ? Result<T>.Success(value!)
            : Result<T>.Failure(error ?? Error.Failure("Unknown", "Deserialized failure without details."));
    }

    public override void Write(Utf8JsonWriter writer, Result<T> result, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isSuccess", result.IsSuccess);

        writer.WritePropertyName("value");
        if (result.IsSuccess)
        {
            JsonSerializer.Serialize(writer, result.Value, options);
        }
        else
        {
            writer.WriteNullValue();
        }

        writer.WritePropertyName("error");
        JsonSerializer.Serialize(writer, result.Error, options);

        writer.WriteEndObject();
    }
}
