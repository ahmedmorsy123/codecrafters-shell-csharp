using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class FalseCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public FalseCommandTests()
    {
        _output = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_output);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _output.Dispose();
    }

    [Fact]
    public void False_WithNoArguments_ProducesNoOutput()
    {
        var cmd = new FalseCommand();
        cmd.Execute(Array.Empty<string>());

        Assert.Equal(string.Empty, _output.ToString());
    }

    [Fact]
    public void False_WithArguments_IgnoresThemAndProducesNoOutput()
    {
        var cmd = new FalseCommand();
        cmd.Execute(new[] { "arg1", "arg2", "arg3" });

        Assert.Equal(string.Empty, _output.ToString());
    }

    [Fact]
    public void False_ReturnsTrueToContinueShell()
    {
        // Note: false command returns true to continue shell execution
        // The "failure" is semantic, not a shell exit
        var cmd = new FalseCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void False_WithManyArguments_StillReturnsTrue()
    {
        var cmd = new FalseCommand();
        var result = cmd.Execute(new[] { "one", "two", "three", "four", "five" });
        Assert.True(result);
    }

    [Fact]
    public void False_WithSpecialCharacters_ReturnsTrue()
    {
        var cmd = new FalseCommand();
        var result = cmd.Execute(new[] { "!@#$%^&*()", "special chars" });
        Assert.True(result);
        Assert.Equal(string.Empty, _output.ToString());
    }
}
