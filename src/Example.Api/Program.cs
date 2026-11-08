using Example.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/hello/{name?}", (string? name) => Greeter.Greet(name ?? string.Empty))
    .WithName("Hello");

app.Run();