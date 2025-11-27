using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ShortenUrlService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "URL Shortener Service is running.");

app.MapPost("/generate", (GenerateLinkRequest request, ShortenUrlService service) =>
{
    return service.CreateShortLink(request);
});

app.MapGet("/{shortCode}", (string shortCode, ShortenUrlService service) =>
{
    var destination = service.GetDestinationUrl(shortCode);
    return destination is not null
        ? Results.Redirect(destination)
        : Results.NotFound();
});


app.Run();
