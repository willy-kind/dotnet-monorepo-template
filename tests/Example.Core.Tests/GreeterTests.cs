using Example.Core;

namespace Example.Core.Tests;

public class GreeterTests
{
    [Theory]
    [InlineData("Willy", "Hello, Willy!")]
    [InlineData("", "Hello, world!")]
    [InlineData("   ", "Hello, world!")]
    public void GreetReturnsGreeting(string name, string expected) =>
        Assert.Equal(expected, Greeter.Greet(name));
}