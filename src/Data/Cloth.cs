namespace SoulDashboard.Data;

public partial class Cloth
{
    public int Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public byte[] Image { get; set; } = [];
}
