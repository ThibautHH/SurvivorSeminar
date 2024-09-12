using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SoulDashboard.Data;
using SoulDashboard.Database.Contexts;
using SoulDashboard.Identity.Authentication.SoulConnection;
using SoulDashboard.Options;

namespace SoulDashboard.Services;

public class SoulConnectionDataService(HttpClient backchannel,
    ApplicationDbContext context,
    UserManager<Employee> userManager,
    IOptionsMonitor<SoulConnectionOptions> options,
    ILogger<SoulConnectionDataService> logger)
{
    private sealed record Entity(int Id);

    private readonly static Func<ApplicationDbContext, DbSet<Employee>> Employees = static c => c.Employees;
    private readonly static Func<ApplicationDbContext, DbSet<Tip>> Tips = static c => c.Tips;
    private readonly static Func<ApplicationDbContext, DbSet<Customer>> Customers = static c => c.Customers;
    private readonly static Func<ApplicationDbContext, DbSet<Encounter>> Encounters = static c => c.Encounters;
    private readonly static Func<ApplicationDbContext, DbSet<Cloth>> Clothes = static c => c.Clothes;
    private readonly static Func<ApplicationDbContext, DbSet<Payment>> Payments = static c => c.Payments;
    private readonly static Func<ApplicationDbContext, DbSet<Event>> Events = static c => c.Events;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task SynchronizeDataAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Synchronizing SoulConnection data.");

        await context.Employees.LoadAsync(cancellationToken);
        await foreach (var update in GetEntities(Employees, cancellationToken)
            .Select(e => AddOrUpdateUserAsync(e, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Tips.LoadAsync(cancellationToken);
        await foreach (var update in GetEntities(Tips, cancellationToken)
            .Select(t => AddOrUpdateAsync(t, Tips, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Customers.LoadAsync(cancellationToken);
        var customers = GetEntities(Customers, cancellationToken);
        await foreach (var update in customers.Select(c => AddOrUpdateAsync(c, Customers, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Encounters.LoadAsync(cancellationToken);
        await foreach (var update in GetEntities(Encounters, cancellationToken)
            .Select(t => AddOrUpdateAsync(t, Encounters, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Clothes.LoadAsync(cancellationToken);
        await foreach (var update in customers
            .SelectManyAwait(c => GetEntities(c, Clothes, cancellationToken))
            .Select(c => AddOrUpdateAsync(c, Clothes, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Payments.LoadAsync(cancellationToken);
        await foreach (var update in customers
            .SelectManyAwait(c => GetEntities(c, Payments, cancellationToken))
            .Select(p => AddOrUpdateAsync(p, Payments, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Events.LoadAsync(cancellationToken);
        await foreach (var update in GetEntities(Events, cancellationToken)
            .Select(e => AddOrUpdateAsync(e, Events, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        foreach (var employee in context.Employees)
            await LoadImageAsync(employee, Employees, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var customer in context.Customers)
            await LoadImageAsync(customer, Customers, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var cloth in context.Clothes)
            await LoadImageAsync(cloth, Clothes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task AddOrUpdateUserAsync(Task<Employee?> employeeTask, CancellationToken cancellationToken = default)
    {
        Employee? employee = null;
        try {
            if (await employeeTask is not Employee e)
                return;
            employee = e;
        } catch (HttpRequestException e) {
            logger.LogError(e, "Failed to fetch employee entity.");
            return;
        }

        if (await userManager.FindByIdAsync(employee!.Id.ToString()) is not Employee contextEmployee)
        {
            await userManager.SetEmailAsync(employee, employee.Email);
            if (!(await userManager.CreateAsync(employee)).Succeeded)
                logger.LogError("Failed to create employee {Id}.", employee.Id);
            else if (!(await userManager.AddLoginAsync(employee,
                new(SoulConnectionDefaults.AuthenticationScheme, employee.Email!, SoulConnectionDefaults.DisplayName))).Succeeded)
                logger.LogError("Failed to add login for employee {Id}.", employee.Id);
        }
        else if (options.CurrentValue.Synchronization.UpdateExisitingRecords)
            context.Entry(contextEmployee).CurrentValues.SetValues(employee);
    }

    private async Task AddOrUpdateAsync<T>(Task<T?> entityTask, Func<ApplicationDbContext, DbSet<T>> setAccessor,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        T? entity = null;
        try {
            if (await entityTask is not T e)
                return;
            entity = e;
        } catch (HttpRequestException e) {
            logger.LogError(e, "Failed to fetch {Type} entity.", typeof(T).Name);
            return;
        }

        if (await setAccessor(context).FindAsync([entity!.Id], cancellationToken) is not T contextEntity)
            setAccessor(context).Add(entity);
        else if (options.CurrentValue.Synchronization.UpdateExisitingRecords)
            setAccessor(context).Entry(contextEntity).CurrentValues.SetValues(entity);
    }

    private static ApiEndpointAttribute GetEndpointAttribute<T>() => typeof(T).GetCustomAttribute<ApiEndpointAttribute>()
        ?? throw new InvalidOperationException($"No {nameof(ApiEndpointAttribute)} found on {typeof(T).Name}.");

    private IAsyncEnumerable<Task<T?>> GetEntities<T>(Func<ApplicationDbContext, DbSet<T>> setAccessor,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        var attribute = GetEndpointAttribute<T>();
        string endpoint = $"/api/{attribute.Endpoint}";

        logger.LogTrace("Getting {Type} entities from {Endpoint}.", typeof(T).Name, endpoint);
        try {
            return attribute.PartialResults
                ? backchannel.GetFromJsonAsAsyncEnumerable<Entity>(endpoint, SerializerOptions, cancellationToken).WhereNotNull()
                    .Select(entity => GetFullEntityAsync(entity, endpoint, setAccessor, cancellationToken))
                : backchannel.GetFromJsonAsAsyncEnumerable<T>(endpoint, SerializerOptions, cancellationToken).Select(Task.FromResult);
        } catch (HttpRequestException e) {
            logger.LogError(e, "Failed to fetch {Type} entities from {Endpoint}.", typeof(T).Name, endpoint);
            return AsyncEnumerable.Empty<Task<T?>>();
        }
    }

    private async ValueTask<IAsyncEnumerable<Task<T?>>> GetEntities<T, TParent>(Task<TParent?> parentTask,
        Func<ApplicationDbContext, DbSet<T>> setAccessor,
        CancellationToken cancellationToken = default)
        where T : class, IEntity
        where TParent : class, IEntity
    {
        if (await parentTask is not TParent parent)
            return AsyncEnumerable.Empty<Task<T?>>();

        ApiEndpointAttribute parentAttribute = GetEndpointAttribute<TParent>(),
            attribute = GetEndpointAttribute<T>();
        string endpoint = $"/api/{parentAttribute.Endpoint}/{parent.Id}/{attribute.Endpoint}";

        logger.LogTrace("Getting {Type} entities of {ParentType} #{Id} from {Endpoint}.",
            typeof(T).Name, typeof(TParent).Name, parent.Id, endpoint);

        try {
            return attribute.PartialResults
                ? backchannel.GetFromJsonAsAsyncEnumerable<Entity>(endpoint, SerializerOptions, cancellationToken).WhereNotNull()
                    .Select(entity => GetFullEntityAsync(entity, endpoint, setAccessor, cancellationToken))
                : backchannel.GetFromJsonAsAsyncEnumerable<T>(endpoint, SerializerOptions, cancellationToken).Select(Task.FromResult);
        } catch (HttpRequestException e) {
            logger.LogError(e, "Failed to fetch {Type} entities of {ParentType} #{Id} from {Endpoint}.",
                typeof(T).Name, typeof(TParent).Name, parent.Id, endpoint);
            return AsyncEnumerable.Empty<Task<T?>>();
        }
    }

    private async Task<T?> GetFullEntityAsync<T>(Entity entity, string endpoint, Func<ApplicationDbContext, DbSet<T>> setAccessor,
        CancellationToken cancellationToken = default)
        where T : class, IEntity => (await setAccessor(context).FindAsync([entity.Id], cancellationToken) is null
                || options.CurrentValue.Synchronization.UpdateExisitingRecords)
            ? await backchannel.GetFromJsonAsync<T>($"{endpoint}/{entity.Id}", SerializerOptions, cancellationToken)
            : null;

    private async Task LoadImageAsync<T>(T entity, Func<ApplicationDbContext, DbSet<T>> setAccessor,
        CancellationToken cancellationToken = default)
        where T : class, IImageEntity
    {
        if (setAccessor(context).Entry(entity).State != EntityState.Added
            && !options.CurrentValue.Synchronization.UpdateExisitingRecords)
            return;

        var endpoint = $"/api/{GetEndpointAttribute<T>().Endpoint}/{entity.Id}/image";

        logger.LogTrace("Loading image for {Type} #{Id} from {Endpoint}.", typeof(T).Name, entity.Id, endpoint);

        using var memoryStream = new MemoryStream();
        Stream stream;
        try {
            stream = await backchannel.GetStreamAsync(endpoint, cancellationToken);
        } catch (HttpRequestException e) {
            logger.LogError(e, "Failed to load image for {Type} #{Id} from {Endpoint}.", typeof(T).Name, entity.Id, endpoint);
            return;
        }
        await stream.CopyToAsync(memoryStream, cancellationToken);
        entity.Image = memoryStream.ToArray();
    }
}
