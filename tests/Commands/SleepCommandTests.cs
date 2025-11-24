using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class SleepCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public SleepCommandTests()
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
    public void Sleep_WithValidSeconds_Sleeps()
    {
        var cmd = new SleepCommand();
        var stopwatch = Stopwatch.StartNew();

        cmd.Execute(new[] { "1" });

        stopwatch.Stop();
        // Allow some tolerance for timing
        Assert.True(stopwatch.ElapsedMilliseconds >= 900); // At least 0.9 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 1500); // Less than 1.5 seconds
    }

    [Fact]
    public void Sleep_WithZero_ReturnsImmediately()
    {
        var cmd = new SleepCommand();
        var stopwatch = Stopwatch.StartNew();

        cmd.Execute(new[] { "0" });

        stopwatch.Stop();
        Assert.True(stopwatch.ElapsedMilliseconds < 100); // Should be very fast
    }

    [Fact]
    public void Sleep_WithNoArguments_PrintsError()
    {
        var cmd = new SleepCommand();
        cmd.Execute(Array.Empty<string>());

        Assert.Contains("missing operand", _output.ToString());
    }

    [Fact]
    public void Sleep_WithInvalidNumber_PrintsError()
    {
        var cmd = new SleepCommand();
        cmd.Execute(new[] { "invalid" });

        var output = _output.ToString();
        Assert.Contains("invalid time interval", output);
        Assert.Contains("invalid", output);
    }

    [Fact]
    public void Sleep_WithNegativeNumber_PrintsError()
    {
        var cmd = new SleepCommand();
        cmd.Execute(new[] { "-5" });

        var output = _output.ToString();
        Assert.Contains("invalid time interval", output);
        Assert.Contains("-5", output);
    }

    [Fact]
    public void Sleep_WithFloatingPoint_PrintsError()
    {
        // For simplicity, only accept integers
        var cmd = new SleepCommand();
        cmd.Execute(new[] { "1.5" });

        var output = _output.ToString();
        Assert.Contains("invalid time interval", output);
    }

    [Fact]
    public void Sleep_ReturnsTrue()
    {
        var cmd = new SleepCommand();
        var result = cmd.Execute(new[] { "0" });
        Assert.True(result);
    }

    [Fact]
    public void Sleep_WithErrorStillReturnsTrue()
    {
        var cmd = new SleepCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void Sleep_WithMultipleArguments_UsesFirst()
    {
        // Should only use first argument, ignore rest
        var cmd = new SleepCommand();
        var stopwatch = Stopwatch.StartNew();

        cmd.Execute(new[] { "1", "2", "3" });

        stopwatch.Stop();
        Assert.True(stopwatch.ElapsedMilliseconds >= 900);
        Assert.True(stopwatch.ElapsedMilliseconds < 1500);
    }

    [Fact]
    public void Sleep_WithLargeNumber_PrintsErrorOrHandlesGracefully()
    {
        var cmd = new SleepCommand();
        var stopwatch = Stopwatch.StartNew();

        // Test with very large number - should error immediately
        var result = cmd.Execute(new[] { "999999" });

        stopwatch.Stop();
        // Should complete very quickly (not actually sleep for 11 days!)
        Assert.True(stopwatch.ElapsedMilliseconds < 100);

        // Should continue shell execution regardless
        Assert.True(result);

        // Should print an error message
        var output = _output.ToString();
        Assert.Contains("invalid time interval", output);
    }
}
