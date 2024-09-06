using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SoulDashboard.Data;
using SoulDashboard.Database.Contexts;

namespace SoulDashboard.Services;

public class SoulConnectionDataService(HttpClient backchannel,
    ApplicationDbContext context,
    ILogger<SoulConnectionDataService> logger)
{
    private sealed record Entity(int Id);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task SynchronizeDataAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Synchronizing SoulConnection data.");

        IAsyncEnumerable<Task<Customer?>> customers;
        IAsyncEnumerable<Task<Employee?>> employees;
        IAsyncEnumerable<Task<Cloth?>> clothes;

        await context.Employees.LoadAsync(cancellationToken);
        employees = GetEntities<Employee>(cancellationToken);
        await foreach (var update in employees.Select(e => AddOrUpdateAsync(e, c => c.Employees, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Tips.LoadAsync(cancellationToken);
        await foreach (var update in GetEntities<Tip>(cancellationToken)
            .Select(t => AddOrUpdateAsync(t, c => c.Tips, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Customers.LoadAsync(cancellationToken);
        customers = GetEntities<Customer>(cancellationToken);
        await foreach (var update in customers.Select(c => AddOrUpdateAsync(c, c => c.Customers, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Clothes.LoadAsync(cancellationToken);
        clothes = customers.SelectManyAwait(c => GetEntitiesAsync<Cloth, Customer>(c, cancellationToken));
        await foreach (var update in clothes.Select(c => AddOrUpdateAsync(c, c => c.Clothes, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Payments.LoadAsync(cancellationToken);
        await foreach (var update in customers
            .SelectManyAwait(c => GetEntitiesAsync<Payment, Customer>(c, cancellationToken))
            .Select(p => AddOrUpdateAsync(p, c => c.Payments, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        await context.Events.LoadAsync(cancellationToken);
        await foreach (var update in GetEntities<Event>(cancellationToken)
            .Select(e => AddOrUpdateAsync(e, c => c.Events, cancellationToken)))
            await update;
        await context.SaveChangesAsync(cancellationToken);

        foreach (var employee in context.Employees)
            await LoadImageAsync(employee, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var customer in context.Customers)
            await LoadImageAsync(customer, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var cloth in context.Clothes)
            await LoadImageAsync(cloth, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
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

        if (await setAccessor(context).FindAsync([entity!.Id], cancellationToken) is null)
            context.Add(entity);
        else
            context.Update(entity);
    }

    private static ApiEndpointAttribute GetEndpointAttribute<T>() => typeof(T).GetCustomAttribute<ApiEndpointAttribute>()
        ?? throw new InvalidOperationException($"No {nameof(ApiEndpointAttribute)} found on {typeof(T).Name}.");

    private IAsyncEnumerable<Task<T?>> GetEntities<T>(CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        var attribute = GetEndpointAttribute<T>();
        string endpoint = $"/api/{attribute.Endpoint}";

        logger.LogTrace("Getting {Type} entities from {Endpoint}.", typeof(T).Name, endpoint);
        try {
            return attribute.PartialResults
                ? backchannel.GetFromJsonAsAsyncEnumerable<Entity>(endpoint, SerializerOptions, cancellationToken).WhereNotNull()
                    .Select(entity => backchannel.GetFromJsonAsync<T>($"{endpoint}/{entity.Id}", SerializerOptions, cancellationToken))
                : backchannel.GetFromJsonAsAsyncEnumerable<T>(endpoint, SerializerOptions, cancellationToken).Select(Task.FromResult);
        } catch (HttpRequestException e) {
            logger.LogError(e, "Failed to fetch {Type} entities from {Endpoint}.", typeof(T).Name, endpoint);
            return AsyncEnumerable.Empty<Task<T?>>();
        }
    }

    private async ValueTask<IAsyncEnumerable<Task<T?>>> GetEntitiesAsync<T, TParent>(Task<TParent?> parentTask, CancellationToken cancellationToken = default)
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
                    .Select(entity => backchannel.GetFromJsonAsync<T>($"{endpoint}/{entity.Id}", SerializerOptions, cancellationToken))
                : backchannel.GetFromJsonAsAsyncEnumerable<T>(endpoint, SerializerOptions, cancellationToken).Select(Task.FromResult);
        } catch (HttpRequestException e) {
            logger.LogError(e, "Failed to fetch {Type} entities of {ParentType} #{Id} from {Endpoint}.",
                typeof(T).Name, typeof(TParent).Name, parent.Id, endpoint);
            return AsyncEnumerable.Empty<Task<T?>>();
        }
    }

    private async Task LoadImageAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : class, IImageEntity
    {
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
