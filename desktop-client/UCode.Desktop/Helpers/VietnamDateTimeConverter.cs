using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UCode.Desktop.Helpers
{
    /// <summary>
    /// Custom JSON converter for DateTime:
    /// - Serialize: Add local timezone offset when sending requests
    /// - Deserialize: Convert UTC to local time when receiving responses
    /// </summary>
    public class VietnamDateTimeConverter : IsoDateTimeConverter
    {
        public VietnamDateTimeConverter()
        {
            // Format: 2024-01-15T14:30:00+07:00
            DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (value is DateTime dateTime)
            {
                // Get local timezone offset (e.g., +07:00 for Vietnam)
                var localOffset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
                
                // Create DateTimeOffset with local timezone
                var dateTimeOffset = new DateTimeOffset(dateTime, localOffset);
                
                // Write as ISO 8601 with timezone: 2024-01-15T14:30:00+07:00
                writer.WriteValue(dateTimeOffset.ToString(DateTimeFormat));
            }
            else
            {
                base.WriteJson(writer, value, serializer);
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.Date)
            {
                var dateTime = (DateTime)reader.Value;
                
                // If UTC, convert to local time
                if (dateTime.Kind == DateTimeKind.Utc)
                {
                    return dateTime.ToLocalTime();
                }
                
                // If unspecified, treat as UTC and convert to local
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
                }
                
                return dateTime;
            }

            if (reader.TokenType == JsonToken.String)
            {
                var dateString = reader.Value?.ToString();
                if (string.IsNullOrEmpty(dateString))
                    return null;

                if (DateTime.TryParse(dateString, out var parsedDate))
                {
                    // If string ends with 'Z' or '+00:00', treat as UTC
                    if (dateString.EndsWith("Z") || dateString.Contains("+00:00"))
                    {
                        return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc).ToLocalTime();
                    }
                    
                    // If no timezone info, treat as UTC and convert to local
                    if (parsedDate.Kind == DateTimeKind.Unspecified)
                    {
                        return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc).ToLocalTime();
                    }
                    
                    return parsedDate;
                }
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
