namespace Example.Job.Integrations.Erp;

public sealed class ErpOptions
{
    public const string Section = "Erp";

    public Uri? BaseAddress { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
