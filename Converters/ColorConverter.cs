using Newtonsoft.Json;
using System;

namespace Reporter_vCLabs
{
    public class ColorConverter : JsonConverter<System.Drawing.Color>
    {
        public override System.Drawing.Color ReadJson(JsonReader reader, Type objectType, System.Drawing.Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string colorString = (string)reader.Value;
            return System.Drawing.Color.FromArgb(int.Parse(colorString));
        }

        public override void WriteJson(JsonWriter writer, System.Drawing.Color value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToArgb().ToString());
        }
    }

}
