using Example.Job.Integrations.Crm;
using Example.Job.Integrations.Erp;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Example.Job.Features.Customers;

public static partial class SyncCustomers
{
    public sealed class Options
    {
        public const string Section = "SyncCustomers";

        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(15);
    }

    public sealed partial class Job(CrmClient crm, ErpClient erp, IOptions<Options> options, TimeProvider time, ILogger<Job> logger) : BackgroundService
    {
        public async Task RunOnceAsync(CancellationToken ct)
        {
            var customers = await crm.GetCustomersAsync(ct);

            foreach (var customer in customers)
            {
                await erp.UpsertCustomerAsync(new ErpCustomer(customer.Id, customer.Name, customer.Email), ct);
            }

            LogSynced(customers.Count);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                {
                    LogFailed(exception);
                }

                await Task.Delay(options.Value.Interval, time, stoppingToken);
            }
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Synced {Count} customers")]
        private partial void LogSynced(int count);

        [LoggerMessage(Level = LogLevel.Error, Message = "Customer sync failed")]
        private partial void LogFailed(Exception exception);
    }
}
