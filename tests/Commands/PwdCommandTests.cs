using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class PwdCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public PwdCommandTests()
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
    public void Pwd_PrintsCurrentDirectory()
    {
        var cmd = new PwdCommand();
        cmd.Execute(Array.Empty<string>());
        Assert.Equal($"{Directory.GetCurrentDirectory()}{Environment.NewLine}", _output.ToString());
    }
}
