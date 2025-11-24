using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class EchoCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public EchoCommandTests()
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
    public void Echo_PrintsArguments()
    {
        var cmd = new EchoCommand();
        cmd.Execute(new[] { "hello", "world" });
        Assert.Equal($"hello world{Environment.NewLine}", _output.ToString());
    }

    [Fact]
    public void Echo_WithNoArguments_PrintsEmptyLine()
    {
        var cmd = new EchoCommand();
        cmd.Execute(Array.Empty<string>());
        Assert.Equal(Environment.NewLine, _output.ToString());
    }

    [Fact]
    public void Echo_WithSpecialCharacters_PrintsCorrectly()
    {
        var cmd = new EchoCommand();
        cmd.Execute(new[] { "hello@#$%^&*()" });
        Assert.Contains("hello@#$%^&*()", _output.ToString());
    }
}
