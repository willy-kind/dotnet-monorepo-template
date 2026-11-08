using Example.Api.Features.Orders;
using Example.Api.Integrations.Erp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddErpClient();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

CreateOrder.Map(app);

app.Run();
