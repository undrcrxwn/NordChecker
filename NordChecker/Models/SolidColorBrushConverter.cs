using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NordChecker.Models
{
    public class SolidColorBrushConverter : JsonConverter<SolidColorBrush>
    {
        public override void WriteJson(JsonWriter writer, SolidColorBrush value, JsonSerializer serializer)
            => writer.WriteValue(value.Dispatcher.Invoke(() => value.ToString()));

        public override SolidColorBrush ReadJson(JsonReader reader, Type objectType, SolidColorBrush existingValue, bool hasExistingValue, JsonSerializer serializer)
            => new BrushConverter().ConvertFrom(reader.Value.ToString()) as SolidColorBrush;
    }
}
