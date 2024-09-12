using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using SoulDashboard.Components;
using SoulDashboard.Components.Account;
using SoulDashboard.Data;
using SoulDashboard.Database.Contexts;
using SoulDashboard.Identity.Authentication.SoulConnection;
using SoulDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddCookie(SoulConnectionDefaults.AuthenticationScheme, SoulConnectionDefaults.DisplayName, SoulConnectionDefaults.Configure)
    .AddIdentityCookies();

builder.Services.AddSoulConnection(builder.Configuration.GetRequiredSection(SoulConnectionDefaults.AuthenticationScheme));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
if (!EF.IsDesignTime)
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
    using (var scope = new ServiceCollection().AddDbContexts(connectionString)
        .BuildServiceProvider()
        .CreateScope())
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
        foreach (var context in (IEnumerable<DbContext>)[
                scope.ServiceProvider.GetRequiredService<IdentityDbContext>(),
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            ])
            await context.Database.MigrateAsync();

builder.Services.AddDatabaseDeveloperPageExceptionFilter()
    .AddDbContexts(connectionString);

builder.Services.AddIdentityCore<Employee>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<Employee>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint()
        .UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

await app.RunAsync();
