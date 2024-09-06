namespace SoulDashboard.Data;

public partial class Encounter
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public byte Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
}
