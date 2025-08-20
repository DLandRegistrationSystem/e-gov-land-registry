using LandRegistry.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register HttpClient + LandApiClient
builder.Services.AddScoped<LandApiClient>();
builder.Services.AddHttpClient("ApiClient", client =>
{
    // Point to the API service, not the web app itself.
    // API launchSettings exposes http://localhost:5034 and https://localhost:7179
    client.BaseAddress = new Uri("http://localhost:5034/");
});

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
