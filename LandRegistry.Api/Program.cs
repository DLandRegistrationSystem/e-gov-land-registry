using LandRegistry.Api.Data;
using Microsoft.EntityFrameworkCore;
// using Microsoft.OpenApi.Models;
// using LandRegistry.Web.Services;
// using LandRegistry.Api.Models; // If models are in Api project

var builder = WebApplication.CreateBuilder(args);

// Add DbContext (SQLite)
builder.Services.AddDbContext<LandDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=land.db"));

// Add controllers
builder.Services.AddControllers();

// Swagger (API docs & testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow Blazor frontend (CORS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

// Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowBlazor");

app.UseHttpsRedirection();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

app.Run();

