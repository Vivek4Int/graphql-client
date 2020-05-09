using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQL.Client.Serializer.SystemTextJson
{
    /// <summary>
    /// A custom JsonConverter for reading the extension fields of <see cref="GraphQLResponse{T}"/> and <see cref="GraphQLError"/>.
    /// </summary>
    /// <remarks>
    /// Taken and modified from GraphQL.SystemTextJson.ObjectDictionaryConverter (GraphQL.NET)
    /// </remarks>
    public class MapConverter : JsonConverter<Map>
    {
        public override Map Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);

            if (doc?.RootElement == null || doc?.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("This converter can only parse when the root element is a JSON Object.");
            }

            return ReadDictionary(doc.RootElement, new Map());
        }

        public override void Write(Utf8JsonWriter writer, Map value, JsonSerializerOptions options)
            => throw new NotImplementedException(
                "This converter currently is only intended to be used to read a JSON object into a strongly-typed representation.");

        private TDictionary ReadDictionary<TDictionary>(JsonElement element, TDictionary result)
            where TDictionary : Dictionary<string, object>
        {
            foreach (var property in element.EnumerateObject())
            {
                result[property.Name] = ReadValue(property.Value);
            }
            return result;
        }

        private IEnumerable<object?> ReadArray(JsonElement value)
        {
            foreach (var item in value.EnumerateArray())
            {
                yield return ReadValue(item);
            }
        }

        private object? ReadValue(JsonElement value)
            => value.ValueKind switch
            {
                JsonValueKind.Array => ReadArray(value).ToList(),
                JsonValueKind.Object => ReadDictionary(value, new Dictionary<string, object>()),
                JsonValueKind.Number => value.ReadNumber(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => throw new InvalidOperationException($"Unexpected value kind: {value.ValueKind}")
            };

        
    }
}
