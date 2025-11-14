using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UCode.Desktop.Helpers
{
    /// <summary>
    /// JsonConverter để tự động chuyển đổi DateTime từ UTC sang giờ địa phương (UTC+7 cho Việt Nam)
    /// </summary>
    public class UtcToLocalDateTimeConverter : IsoDateTimeConverter
    {
        public UtcToLocalDateTimeConverter()
        {
            // Định dạng ISO 8601
            DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.Date)
            {
                var dateTime = (DateTime)reader.Value;
                
                // Nếu DateTime là UTC, chuyển sang giờ địa phương
                if (dateTime.Kind == DateTimeKind.Utc)
                {
                    return dateTime.ToLocalTime();
                }
                
                // Nếu không có timezone info, coi như UTC và chuyển sang local
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
                }
                
                return dateTime;
            }

            if (reader.TokenType == JsonToken.String)
            {
                var dateString = reader.Value.ToString();
                
                if (DateTime.TryParse(dateString, out var parsedDate))
                {
                    // Nếu string có 'Z' ở cuối hoặc '+00:00', coi như UTC
                    if (dateString.EndsWith("Z") || dateString.Contains("+00:00"))
                    {
                        return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc).ToLocalTime();
                    }
                    
                    // Nếu không có timezone, coi như UTC
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
