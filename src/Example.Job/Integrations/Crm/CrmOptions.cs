namespace Example.Job.Integrations.Crm;

public sealed class CrmOptions
{
    public const string Section = "Crm";

    public Uri? BaseAddress { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
