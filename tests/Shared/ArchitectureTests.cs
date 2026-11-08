using System.Reflection;
using System.Text.RegularExpressions;

using ArchUnitNET.xUnitV3;

using Microsoft.Extensions.Hosting;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

using Architecture = ArchUnitNET.Domain.Architecture;
using ArchLoader = ArchUnitNET.Loader.ArchLoader;
using IType = ArchUnitNET.Domain.IType;

namespace Example.Testing;

/// <summary>
/// Linked into every test project; checks the app that project tests (the one whose Program it sees).
/// </summary>
public class ArchitectureTests
{
    private const string Features = @"\.Features(\.|$)";
    private const string Common = @"\.Common(\.|$)";
    private const string Domain = @"\.Domain(\.|$)";
    private const string Integrations = @"\.Integrations(\.|$)";

    private static readonly Assembly App = typeof(Program).Assembly;
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(App).Build();

    [Fact]
    public void SlicesNeverReferenceEachOther()
    {
        var violations =
            from type in Architecture.Types
            let source = SliceOf(type)
            where source is not null
            from dependency in type.Dependencies
            let target = SliceOf(dependency.Target)
            where target is not null && target != source
            select $"{type.FullName} -> {dependency.Target.FullName}";

        AssertNone("Slices must not reference other slices", violations);
    }

    [Theory]
    [InlineData(Common)]
    [InlineData(Domain)]
    [InlineData(Integrations)]
    public void SharedCodeNeverReferencesFeatures(string shared)
    {
        Types().That().ResideInNamespaceMatching(shared)
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(Features))
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void IntegrationsNeverReferenceDomain()
    {
        Types().That().ResideInNamespaceMatching(Integrations)
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(Domain))
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void EverySliceHasATestClass()
    {
        var testClasses = typeof(ArchitectureTests).Assembly.GetTypes().Select(t => t.FullName).ToHashSet();

        var missing = App.GetTypes()
            .Where(t => !t.IsNested && !t.Name.StartsWith('<') && Regex.IsMatch(t.Namespace ?? "", Features))
            .Select(ExpectedTestClass)
            .Where(name => !testClasses.Contains(name));

        AssertNone("Slices without a test class", missing);
    }

    [Fact]
    public void EveryJobHasRunOnceAsync()
    {
        const BindingFlags anyMethod = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        var missing = App.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BackgroundService)) && t.GetMethod("RunOnceAsync", anyMethod) is null)
            .Select(t => t.FullName!);

        AssertNone("Jobs without RunOnceAsync", missing);
    }

    private static void AssertNone(string rule, IEnumerable<string> violations)
    {
        var list = violations.Distinct().ToList();
        Assert.True(list.Count == 0, $"{rule}:{Environment.NewLine}{string.Join(Environment.NewLine, list)}");
    }

    // The top-level slice class a type belongs to, or null when it isn't in Features.
    private static string? SliceOf(IType type) => Regex.IsMatch(type.Namespace.FullName, Features) ? type.FullName.Split('+', '/')[0] : null;

    // Example.Api.Features.Orders.CreateOrder -> Example.Api.Tests.Features.Orders.CreateOrderTests
    private static string ExpectedTestClass(Type slice)
    {
        var app = App.GetName().Name!;
        return $"{app}.Tests{slice.FullName![app.Length..]}Tests";
    }
}
