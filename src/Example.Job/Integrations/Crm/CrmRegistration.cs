using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Example.Job.Integrations.Crm;

public static class CrmRegistration
{
    public static IServiceCollection AddCrmClient(this IServiceCollection services)
    {
        services.AddOptions<CrmOptions>()
            .BindConfiguration(CrmOptions.Section)
            .Validate(options => options.BaseAddress is not null, $"{CrmOptions.Section}:BaseAddress is required.")
            .ValidateOnStart();

        services.AddHttpClient<CrmClient>((provider, http) =>
        {
            var options = provider.GetRequiredService<IOptions<CrmOptions>>().Value;
            http.BaseAddress = options.BaseAddress;
            http.Timeout = options.Timeout;
        });

        return services;
    }
}
