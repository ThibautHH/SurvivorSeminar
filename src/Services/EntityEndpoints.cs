using SoulDashboard.Services;

namespace SoulDashboard.Data;

[ApiEndpoint("clothes", partialResults: false)]
public partial class Cloth : IImageEntity;

[ApiEndpoint("customers")]
public partial class Customer : IImageEntity;

[ApiEndpoint("employees")]
public partial class Employee : IImageEntity;

[ApiEndpoint("encounters")]
public partial class Encounter : IEntity;

[ApiEndpoint("events")]
public partial class Event : IEntity;

[ApiEndpoint("payments_history", partialResults: false)]
public partial class Payment : IEntity;

[ApiEndpoint("tips", partialResults: false)]
public partial class Tip : IEntity;
