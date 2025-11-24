using System;
using Xunit;

namespace Tests.Core;

public class ModelsTests
{
    [Fact]
    public void CommandNameAttribute_WithEmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new CommandNameAttribute(""));
    }

    [Fact]
    public void CommandNameAttribute_WithValidName_StoresName()
    {
        var attr = new CommandNameAttribute("test");
        Assert.Equal("test", attr.Name);
    }

    [Fact]
    public void ShellException_WithMessage_CreatesException()
    {
        var ex = new ShellException("test message");
        Assert.Equal("test message", ex.Message);
    }

    [Fact]
    public void CommandNotFoundException_WithCommandName_FormatsMessage()
    {
        var ex = new CommandNotFoundException("testcmd");
        Assert.Contains("testcmd", ex.Message);
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void ExecutionException_WithCommandAndInner_FormatsMessage()
    {
        var inner = new Exception("inner error");
        var ex = new ExecutionException("testcmd", inner);
        Assert.Contains("testcmd", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void RedirectionInfo_Properties_WorkCorrectly()
    {
        var redirection = new RedirectionInfo
        {
            StdoutFile = "out.txt",
            StdoutAppend = true,
            StderrFile = "err.txt",
            StderrAppend = false
        };

        Assert.Equal("out.txt", redirection.StdoutFile);
        Assert.True(redirection.StdoutAppend);
        Assert.Equal("err.txt", redirection.StderrFile);
        Assert.False(redirection.StderrAppend);
        Assert.True(redirection.HasStdoutRedirection);
        Assert.True(redirection.HasStderrRedirection);
    }

    [Fact]
    public void RedirectionInfo_NoRedirection_ReturnsFalse()
    {
        var redirection = new RedirectionInfo();
        Assert.False(redirection.HasStdoutRedirection);
        Assert.False(redirection.HasStderrRedirection);
    }

    [Fact]
    public void Command_ToString_FormatsCorrectly()
    {
        var cmd = new Command("echo", new[] { "hello", "world" });
        Assert.Equal("echo hello world", cmd.ToString());
    }

    [Fact]
    public void Command_WithNoArgs_ToStringReturnsCommandName()
    {
        var cmd = new Command("pwd", Array.Empty<string>());
        Assert.Equal("pwd", cmd.ToString());
    }

    [Fact]
    public void Command_Properties_WorkCorrectly()
    {
        var redirection = new RedirectionInfo { StdoutFile = "out.txt" };
        var cmd = new Command("echo", new[] { "test" }, redirection);

        Assert.Equal("echo", cmd.CommandName);
        Assert.Single(cmd.Args);
        Assert.Equal("test", cmd.Args[0]);
        Assert.Same(redirection, cmd.Redirection);
    }

    [Fact]
    public void Pipeline_ToString_FormatsCorrectly()
    {
        var pipeline = new Pipeline(new[]
        {
            new Command("echo", new[] { "hello" }),
            new Command("grep", new[] { "h" })
        });

        var result = pipeline.ToString();
        Assert.Contains("echo hello", result);
        Assert.Contains("|", result);
        Assert.Contains("grep h", result);
    }

    [Fact]
    public void Pipeline_IsSingleCommand_WorksCorrectly()
    {
        var single = new Pipeline(new[] { new Command("echo", new[] { "test" }) });
        Assert.True(single.IsSingleCommand);

        var multiple = new Pipeline(new[]
        {
            new Command("echo", new[] { "test" }),
            new Command("grep", new[] { "t" })
        });
        Assert.False(multiple.IsSingleCommand);
    }
}
