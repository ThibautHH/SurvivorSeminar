using Microsoft.AspNetCore.Identity;

namespace SoulDashboard.Data;

public partial class Employee : IdentityUser<int>
{
    [ProtectedPersonalData]
    public string Name { get; set; } = string.Empty;

    [ProtectedPersonalData]
    public string Surname { get; set; } = string.Empty;

    [ProtectedPersonalData]
    public DateOnly BirthDate { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string Work { get; set; } = string.Empty;

    public byte[] Image { get; set; } = [];

    public IList<Customer> Customers { get; set; } = [];
}
