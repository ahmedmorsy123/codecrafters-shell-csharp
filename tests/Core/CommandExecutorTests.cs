using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Tests.Core;

public class CommandExecutorTests : IDisposable
{
#nullable enable
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public CommandExecutorTests()
    {
        // Clear static state before each test
        Autocomplete.Clear();
        PipelineHistory.ClearHistory();
        _output = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_output);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _output.Dispose();
        Autocomplete.Clear();
        PipelineHistory.ClearHistory();
    }

    [Fact]
    public void FindExecutableInPath_FindsCmd()
    {
        if (OperatingSystem.IsWindows())
        {
            string? path = CommandExecutor.FindExecutableInPath("cmd.exe");
            Assert.NotNull(path);
            Assert.True(File.Exists(path));
        }
    }

    [Fact]
    public void ExecuteExternal_RunsCmdEcho()
    {
        if (OperatingSystem.IsWindows())
        {
            // Capture output
            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);

            try
            {
                var executor = new CommandExecutor();
                var command = new Command("cmd.exe", new[] { "/c", "echo", "external_test" });
                executor.Execute(command);

                Assert.Contains("external_test", sw.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }

    [Fact]
    public void Execute_WithEmptyCommandName_ReturnsTrue()
    {
        var executor = new CommandExecutor();
        var cmd = new Command("", Array.Empty<string>());
        var result = executor.Execute(cmd);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithWhitespaceCommandName_ReturnsTrue()
    {
        var executor = new CommandExecutor();
        var cmd = new Command("   ", Array.Empty<string>());
        var result = executor.Execute(cmd);
        Assert.True(result);
    }

    [Fact]
    public void GetBuiltinCommands_ReturnsAllCommands()
    {
        var commands = CommandExecutor.GetBuiltinCommands().ToList();
        Assert.Contains("echo", commands, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("pwd", commands, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("cd", commands, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("exit", commands, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("type", commands, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("history", commands, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsCommand_WithBuiltin_ReturnsTrue()
    {
        Assert.True(CommandExecutor.IsCommand("echo"));
        Assert.True(CommandExecutor.IsCommand("ECHO")); // Case insensitive
        Assert.True(CommandExecutor.IsCommand("pwd"));
        Assert.True(CommandExecutor.IsCommand("cd"));
    }

    [Fact]
    public void IsCommand_WithNonBuiltin_ReturnsFalse()
    {
        Assert.False(CommandExecutor.IsCommand("nonexistent"));
        Assert.False(CommandExecutor.IsCommand(""));
    }

    [Fact]
    public void ExecutePipeline_WithSingleCommand_ExecutesCommand()
    {
        var executor = new CommandExecutor();
        var pipeline = new Pipeline(new[] { new Command("echo", new[] { "single" }) });
        var result = executor.ExecutePipeline(pipeline);
        Assert.True(result);
        Assert.Contains("single", _output.ToString());
    }

    [Fact]
    public void Execute_WithOutputRedirection_WritesToFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var executor = new CommandExecutor();
            var redirection = new RedirectionInfo
            {
                StdoutFile = tempFile,
                StdoutAppend = false
            };
            var cmd = new Command("echo", new[] { "redirected" }, redirection);

            executor.Execute(cmd);

            var content = File.ReadAllText(tempFile);
            Assert.Contains("redirected", content);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void FindExecutableInPath_WithNonexistent_ReturnsNull()
    {
        var result = CommandExecutor.FindExecutableInPath("nonexistent_executable_xyz");
        Assert.Null(result);
    }

    [Fact]
    public void Execute_WithNonexistentExternal_ThrowsException()
    {
        var executor = new CommandExecutor();
        var cmd = new Command("nonexistent_cmd_xyz", Array.Empty<string>());

        // CommandExecutor throws CommandNotFoundException for unknown commands
        Assert.Throws<CommandNotFoundException>(() => executor.Execute(cmd));
    }

    [Fact]
    public void HasExecutePermission_WithValidFile_ReturnsTrue()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var result = CommandExecutor.HasExecutePermission(tempFile);

            if (OperatingSystem.IsWindows())
            {
                Assert.True(result); // Windows just checks existence
            }
            else
            {
                // Unix might be false if no execute permission
                Assert.True(result == true || result == false);
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void HasExecutePermission_WithNonexistentFile_ReturnsFalse()
    {
        var result = CommandExecutor.HasExecutePermission("/nonexistent/file/path");
        Assert.False(result);
    }

    [Fact]
    public void ExecutePipeline_WithExitCommand_ReturnsFalse()
    {
        var executor = new CommandExecutor();
        var cmd = new Command("exit", new[] { "0" });
        var pipeline = new Pipeline(new[] { cmd });

        var result = executor.ExecutePipeline(pipeline);
        Assert.False(result);
    }

    [Fact]
    public void ExecutePipeline_WithBuiltinPipedToBuiltin_Works()
    {
        var executor = new CommandExecutor();
        var tempFile = Path.GetTempFileName();

        try
        {
            File.WriteAllText(tempFile, "line1\nline2\nline3");

            // This would need actual pipe support which might not be fully implemented
            // Test just verifies it doesn't crash
            var cmd1 = new Command("pwd", Array.Empty<string>());
            var cmd2 = new Command("echo", new[] { "test" });
            var pipeline = new Pipeline(new[] { cmd1, cmd2 });

            var result = executor.ExecutePipeline(pipeline);
            Assert.True(result);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Execute_WithStderrRedirection_CreatesFile()
    {
        var executor = new CommandExecutor();
        var tempFile = Path.GetTempFileName();

        try
        {
            // This tests the redirection logic path
            var redirection = new RedirectionInfo
            {
                StderrFile = tempFile,
                StderrAppend = false
            };
            var cmd = new Command("echo", new[] { "test" }, redirection);

            executor.Execute(cmd);

            Assert.True(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void FindExecutableInPath_WithEmptyPATH_ReturnsNull()
    {
        var originalPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", "");
            var result = CommandExecutor.FindExecutableInPath("echo");
            Assert.Null(result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }

    [Fact]
    public void FindExecutableInPath_WithNullPATH_ReturnsNull()
    {
        var originalPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", null);
            var result = CommandExecutor.FindExecutableInPath("echo");
            Assert.Null(result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }

    [Fact]
    public void FindExecutableInPath_SkipsNonexistentDirectories()
    {
        var originalPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", "/nonexistent/dir1" + Path.PathSeparator + "/nonexistent/dir2");
            var result = CommandExecutor.FindExecutableInPath("echo");
            Assert.Null(result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }
}
