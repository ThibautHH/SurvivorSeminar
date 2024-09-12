using Microsoft.AspNetCore.Identity;

namespace SoulDashboard.Data;

public partial class Customer
{
    public int Id { get; set; }

    [ProtectedPersonalData]
    public string Email { get; set; } = string.Empty;

    [ProtectedPersonalData]
    public string Name { get; set; } = string.Empty;

    [ProtectedPersonalData]
    public string Surname { get; set; } = string.Empty;

    [ProtectedPersonalData]
    public DateOnly BirthDate { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string AstrologicalSign { get; set; } = string.Empty;

    [ProtectedPersonalData]
    public string PhoneNumber { get; set; } = string.Empty;

    [ProtectedPersonalData]
    public string Address { get; set; } = string.Empty;

    public byte[] Image { get; set; } = [];

    public IList<Encounter> Encounters { get; set; } = [];

    public IList<Payment> Payments { get; set; } = [];
}
