
namespace ODEliteTracker.Helpers.JsonCoverters
{
    using System;
    using Newtonsoft.Json;

    public class BoolToIntConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(int)) || objectType.IsAssignableFrom(typeof(bool));
        }
    
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is bool v)
            {
                writer.WriteValue(v ? 1 : 0);
                return;
            }
            if (value is int i)
            {
                writer.WriteValue(i);
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt32(reader.Value);
            }
            if (reader.TokenType == JsonToken.Boolean)
            {
                if (reader.Value == null)
                    return 0;
                return (bool)reader.Value ? 1 : 0;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                if (reader.Value == null)
                    return 0;
                // Handle "0", "1", "true", "false"
                var str = reader.Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(str))
                    return 0;

                if (int.TryParse(str, out int intVal))
                    return intVal;
                if (bool.TryParse(str, out bool boolVal))
                    return boolVal ? 1 : 0;
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing.");
        }        
    }
}
