using Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register HttpClient for API requests
string apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5271";
string apiKey = builder.Configuration.GetValue<string>("ApiKey") ?? string.Empty;

builder.Services.AddScoped(sp =>
{
    HttpClient httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    if (!string.IsNullOrEmpty(apiKey))
    {
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
    }
    return httpClient;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
