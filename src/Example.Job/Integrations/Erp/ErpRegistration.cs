using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Example.Job.Integrations.Erp;

public static class ErpRegistration
{
    public static IServiceCollection AddErpClient(this IServiceCollection services)
    {
        services.AddOptions<ErpOptions>()
            .BindConfiguration(ErpOptions.Section)
            .Validate(options => options.BaseAddress is not null, $"{ErpOptions.Section}:BaseAddress is required.")
            .ValidateOnStart();

        services.AddHttpClient<ErpClient>((provider, http) =>
        {
            var options = provider.GetRequiredService<IOptions<ErpOptions>>().Value;
            http.BaseAddress = options.BaseAddress;
            http.Timeout = options.Timeout;
        });

        return services;
    }
}
