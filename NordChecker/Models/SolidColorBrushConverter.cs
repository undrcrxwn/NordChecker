using Newtonsoft.Json;
using System;
using System.Windows.Media;

namespace NordChecker.Models
{
    public class SolidColorBrushConverter : JsonConverter<SolidColorBrush>
    {
        private readonly BrushConverter _BrushConverter = new();

        public override void WriteJson(JsonWriter writer, SolidColorBrush value, JsonSerializer serializer)
        {
            string hex = value.Dispatcher is null
                ? value.ToString()
                : value.Dispatcher.Invoke(value.ToString);
            writer.WriteValue(hex);
        }

        public override SolidColorBrush ReadJson(
            JsonReader reader, Type objectType, SolidColorBrush existingValue,
            bool hasExistingValue, JsonSerializer serializer) =>
            _BrushConverter.ConvertFrom(reader.Value) as SolidColorBrush;
    }
}
