using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class WhoamiCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public WhoamiCommandTests()
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
    public void Whoami_DisplaysUsername()
    {
        var cmd = new WhoamiCommand();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString().Trim();
        Assert.NotEmpty(output);

        // Verify it's a reasonable username (no whitespace, not empty)
        Assert.DoesNotContain(Environment.NewLine, output);
        Assert.True(output.Length > 0);
    }

    [Fact]
    public void Whoami_WithArguments_IgnoresThemAndDisplaysUsername()
    {
        var cmd = new WhoamiCommand();
        cmd.Execute(new[] { "arg1", "arg2" });

        var output = _output.ToString().Trim();
        Assert.NotEmpty(output);
    }

    [Fact]
    public void Whoami_MatchesEnvironmentVariable()
    {
        var cmd = new WhoamiCommand();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString().Trim();
        var username = Environment.GetEnvironmentVariable("USERNAME")
                    ?? Environment.GetEnvironmentVariable("USER");

        if (username != null)
        {
            Assert.Equal(username, output);
        }
        else
        {
            // If no username found, should print error
            Assert.Contains("cannot find name", output);
        }
    }

    [Fact]
    public void Whoami_ReturnsTrue()
    {
        var cmd = new WhoamiCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void Whoami_ProducesOnlyOneLine()
    {
        var cmd = new WhoamiCommand();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        var lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(lines);
    }
}
