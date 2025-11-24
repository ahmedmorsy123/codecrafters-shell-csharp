using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class TypeCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public TypeCommandTests()
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
    public void Type_IdentifiesBuiltin()
    {
        var cmd = new TypeCommand();
        cmd.Execute(new[] { "echo" });
        Assert.Equal($"echo is a shell builtin{Environment.NewLine}", _output.ToString());
    }

    [Fact]
    public void Type_IdentifiesExternalCommand()
    {
        if (OperatingSystem.IsWindows())
        {
            var cmd = new TypeCommand();
            cmd.Execute(new[] { "cmd.exe" });
            Assert.Contains("cmd.exe is", _output.ToString());
        }
    }

    [Fact]
    public void TypeCommand_WithoutArgument_PrintsError()
    {
        var cmd = new TypeCommand();
        cmd.Execute(Array.Empty<string>());
        Assert.Contains("missing argument", _output.ToString());
    }

    [Fact]
    public void TypeCommand_WithBuiltinCommand_IdentifiesBuiltin()
    {
        // Need to initialize CommandExecutor to register builtin commands
        var executor = new CommandExecutor();

        var cmd = new TypeCommand();
        cmd.Execute(new[] { "echo" });
        Assert.Contains("shell builtin", _output.ToString());
    }

    [Fact]
    public void TypeCommand_WithNonexistentCommand_PrintsNotFound()
    {
        var cmd = new TypeCommand();
        cmd.Execute(new[] { "nonexistent_command_xyz" });
        Assert.Contains("not found", _output.ToString());
    }
}
