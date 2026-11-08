using Example.Job.Features.Customers;
using Example.Job.Integrations.Crm;
using Example.Job.Integrations.Erp;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddCrmClient();
builder.Services.AddErpClient();

builder.Services.AddOptions<SyncCustomers.Options>().BindConfiguration(SyncCustomers.Options.Section);
builder.Services.AddHostedService<SyncCustomers.Job>();

var host = builder.Build();

host.Run();
