using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;
using Shortener.Api.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShortenerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "URL Shortener Service is running.");

app.MapGet("/health", async (ShortenerDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { Status = "Healthy", Database = "Connected" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

app.MapCreateShortUrl();
app.MapRedirectUrl();

app.Run();
