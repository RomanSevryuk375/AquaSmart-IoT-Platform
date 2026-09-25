using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Domain.Results.Converters;

public sealed class ResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(ResultOfTJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
