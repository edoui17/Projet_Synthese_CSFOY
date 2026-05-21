using Web.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Web.Services;
using Web.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Authentication services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthService>();

// Register HttpClient with SessionTokenHandler
string apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5271";
string apiKey = builder.Configuration.GetValue<string>("ApiKey") ?? string.Empty;

builder.Services.AddTransient<SessionTokenHandler>();

builder.Services.AddHttpClient("IslandSurvivorApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
    }
}).AddHttpMessageHandler<SessionTokenHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("IslandSurvivorApi"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
