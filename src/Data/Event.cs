using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoulDashboard.Data;

public class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    [JsonConverter(typeof(FromMinutesConverter))]
    public TimeSpan Duration { get; set; }

    public ushort MaxParticipants { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public string LocationX { get; set; } = string.Empty;

    public string LocationY { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    private sealed class FromMinutesConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            TimeSpan.FromMinutes(reader.GetInt64());

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value.Minutes);
    }
}
