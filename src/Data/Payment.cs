namespace SoulDashboard.Data;

public class Payment
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public float Amount { get; set; }

    public string Comment { get; set; } = string.Empty;
}
