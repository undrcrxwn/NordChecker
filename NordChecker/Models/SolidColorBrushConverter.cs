using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace NordChecker.Models
{
    public class SolidColorBrushConverter : JsonConverter<SolidColorBrush>
    {
        public override void WriteJson(JsonWriter writer, SolidColorBrush value, JsonSerializer serializer)
        {
            string hex = value.Dispatcher is null
                ? value.ToString()
                : value.Dispatcher.Invoke(value.ToString);
            writer.WriteValue(hex);
        }

        public override SolidColorBrush ReadJson(JsonReader reader, Type objectType, SolidColorBrush existingValue, bool hasExistingValue, JsonSerializer serializer)
            => new BrushConverter().ConvertFrom(reader.Value.ToString()) as SolidColorBrush;
    }
}
