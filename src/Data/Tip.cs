using System.Text.Json.Serialization;

namespace SoulDashboard.Data;

public partial class Tip
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("tip")]
    public string Content { get; set; } = string.Empty;
}
