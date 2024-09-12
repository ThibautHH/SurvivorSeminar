namespace SoulDashboard.Services;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ApiEndpointAttribute(string endpoint, bool partialResults = true) : Attribute
{
    public string Endpoint { get; } = endpoint;

    public bool PartialResults { get; } = partialResults;
}
