using Newtonsoft.Json;
using OpenQA.Selenium;

namespace GetWeixinToken
{
    /// <summary>
    /// 构建该类一方面是为了加快Json的序列化和反序列化速度另一方面是Cookie因为不包含构造函数所以必须使用JsonConverter自己处理
    /// </summary>
    public class CookieConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Cookie);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            string name = null;
            string value = null;
            string domian = null;
            string path = null;
            DateTime? expiry = null;
            bool secure = false;
            bool httpOnly = false;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    return new Cookie(name, value, domian, path, expiry, secure, httpOnly, null);

                if (reader.TokenType != JsonToken.PropertyName)
                    continue;

                var propertyName = (string)reader.Value;
                reader.Read();

                switch (propertyName)
                {
                    case "name":
                        name = (string)reader.Value;
                        break;
                    case "value":
                        value = (string)reader.Value;
                        break;
                    case "domain":
                        domian = (string)reader.Value;
                        break;
                    case "path":
                        path = (string)reader.Value;
                        break;
                    case "expiry":
                        var expiryTicks = (long)reader.Value;
                        var expiryTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryTicks);
                        expiry = expiryTime;
                        break;
                    case "secure":
                        secure = (bool)reader.Value;
                        break;
                    case "httpOnly":
                        httpOnly = (bool)reader.Value;
                        break;
                }
            }

            return new Cookie(name, value, domian, path, expiry, secure, httpOnly, null);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var cookie = (Cookie)value;

            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(cookie.Name);
            writer.WritePropertyName("value");
            writer.WriteValue(cookie.Value);
            writer.WritePropertyName("domain");
            writer.WriteValue(cookie.Domain);
            writer.WritePropertyName("path");
            writer.WriteValue(cookie.Path);
            writer.WritePropertyName("expiry");
            if(cookie.Expiry.HasValue)
            {
                writer.WriteValue((long)(cookie.Expiry - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Value.TotalSeconds);
            }
            else
            {
                writer.WriteValue(0);
            }
            writer.WritePropertyName("secure");
            writer.WriteValue(cookie.Secure);
            writer.WritePropertyName("httpOnly");
            writer.WriteValue(cookie.IsHttpOnly);
            writer.WritePropertyName("sameSite");
            writer.WriteValue(cookie.SameSite);
            writer.WriteEndObject();
        }
    }

}
