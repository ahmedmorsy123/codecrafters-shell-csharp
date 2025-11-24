using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class TrueCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public TrueCommandTests()
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
    public void True_WithNoArguments_ProducesNoOutput()
    {
        var cmd = new TrueCommand();
        cmd.Execute(Array.Empty<string>());

        Assert.Equal(string.Empty, _output.ToString());
    }

    [Fact]
    public void True_WithArguments_IgnoresThemAndProducesNoOutput()
    {
        var cmd = new TrueCommand();
        cmd.Execute(new[] { "arg1", "arg2", "arg3" });

        Assert.Equal(string.Empty, _output.ToString());
    }

    [Fact]
    public void True_ReturnsTrue()
    {
        var cmd = new TrueCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void True_WithManyArguments_StillReturnsTrue()
    {
        var cmd = new TrueCommand();
        var result = cmd.Execute(new[] { "one", "two", "three", "four", "five" });
        Assert.True(result);
    }

    [Fact]
    public void True_WithSpecialCharacters_ReturnsTrue()
    {
        var cmd = new TrueCommand();
        var result = cmd.Execute(new[] { "!@#$%^&*()", "special chars" });
        Assert.True(result);
        Assert.Equal(string.Empty, _output.ToString());
    }
}
