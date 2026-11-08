namespace Example.Core;

public static class Greeter
{
    public static string Greet(string name) =>
        string.IsNullOrWhiteSpace(name) ? "Hello, world!" : $"Hello, {name}!";
}