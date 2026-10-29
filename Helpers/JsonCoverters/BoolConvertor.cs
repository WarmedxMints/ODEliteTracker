
namespace ODEliteTracker.Helpers.JsonCoverters
{
    using System;
    using Newtonsoft.Json;

    public class BoolConvertor : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(int)) || objectType.IsAssignableFrom(typeof(bool)) || objectType.IsAssignableFrom(typeof(string));
        }
    
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is bool v)
            {
                writer.WriteValue(v);
                return;
            }
            if (value is int i)
            {
                writer.WriteValue(i == 1);
                return;
            }
            if (value is string s)
            {
                s = s.Trim();
                if (string.IsNullOrEmpty(s))
                {
                    writer.WriteValue(false);
                    return;
                }

                if (int.TryParse(s, out int intVal))
                {
                    writer.WriteValue(intVal == 1);
                    return;
                }
                if (bool.TryParse(s, out bool boolVal))
                {
                    writer.WriteValue(boolVal);
                    return;
                }
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt32(reader.Value) == 1;
            }
            if (reader.TokenType == JsonToken.Boolean)
            {
                if (reader.Value == null)
                    return false;
                return (bool)reader.Value;
            }
            if (reader.TokenType == JsonToken.String)
            {
                if (reader.Value == null)
                    return 0;
                // Handle "0", "1", "true", "false"
                var str = reader.Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(str))
                    return false;

                if (int.TryParse(str, out int intVal))
                    return intVal == 1;
                if (bool.TryParse(str, out bool boolVal))
                    return boolVal;
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing.");
        }        
    }
}
