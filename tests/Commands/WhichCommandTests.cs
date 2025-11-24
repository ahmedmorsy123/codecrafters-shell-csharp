using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class WhichCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;
    private readonly CommandExecutor _executor;

    public WhichCommandTests()
    {
        _output = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_output);

        // Initialize CommandExecutor to register all builtin commands
        _executor = new CommandExecutor();
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _output.Dispose();
    }

    [Fact]
    public void Which_WithBuiltinCommand_DisplaysBuiltinMessage()
    {
        var cmd = new WhichCommand();
        cmd.Execute(new[] { "echo" });

        var output = _output.ToString();
        Assert.Contains("shell builtin", output.ToLower());
    }

    [Fact]
    public void Which_WithMultipleBuiltins_DisplaysMultipleMessages()
    {
        var cmd = new WhichCommand();
        cmd.Execute(new[] { "echo", "cd", "pwd" });

        var output = _output.ToString();
        Assert.Contains("echo", output);
        Assert.Contains("cd", output);
        Assert.Contains("pwd", output);
    }

    [Fact]
    public void Which_WithNoArguments_PrintsError()
    {
        var cmd = new WhichCommand();
        cmd.Execute(Array.Empty<string>());

        Assert.Contains("missing argument", _output.ToString());
    }

    [Fact]
    public void Which_WithNonexistentCommand_ProducesNoOutput()
    {
        var cmd = new WhichCommand();
        cmd.Execute(new[] { "nonexistent_command_xyz_12345" });

        var output = _output.ToString();
        // Should not produce output for non-existent commands
        // If this fails, the output is: [output]
        Assert.True(string.IsNullOrWhiteSpace(output), $"Expected no output, but got: [{output}]");
    }

    [Fact]
    public void Which_WithMixedExistingAndNonexisting_DisplaysOnlyExisting()
    {
        var cmd = new WhichCommand();
        cmd.Execute(new[] { "echo", "nonexistent_xyz" });

        var output = _output.ToString();
        Assert.Contains("echo", output);
        // Should not fail completely, just skip nonexistent
    }

    [Fact]
    public void Which_ReturnsTrue()
    {
        var cmd = new WhichCommand();
        var result = cmd.Execute(new[] { "echo" });
        Assert.True(result);
    }

    [Fact]
    public void Which_WithErrorStillReturnsTrue()
    {
        var cmd = new WhichCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void Which_WithExitCommand_DisplaysBuiltin()
    {
        var cmd = new WhichCommand();
        cmd.Execute(new[] { "exit" });

        var output = _output.ToString();
        Assert.Contains("exit", output);
        Assert.Contains("builtin", output.ToLower());
    }

    [Fact]
    public void Which_WithHistoryCommand_DisplaysBuiltin()
    {
        var cmd = new WhichCommand();
        cmd.Execute(new[] { "history" });

        var output = _output.ToString();
        Assert.Contains("history", output);
        Assert.Contains("builtin", output.ToLower());
    }

    [Fact]
    public void Which_CanFindExternalCommands()
    {
        // This test is environment-dependent
        // On Windows, try to find a common command
        var cmd = new WhichCommand();
        cmd.Execute(new[] { "cmd" });

        var output = _output.ToString();
        // Should either find it in PATH or show it's a builtin
        Assert.NotEmpty(output);
    }
}
