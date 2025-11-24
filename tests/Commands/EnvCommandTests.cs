using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class EnvCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public EnvCommandTests()
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
    public void Env_WithNoArguments_DisplaysAllEnvironmentVariables()
    {
        var cmd = new EnvCommand();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        // Check for common environment variables
        Assert.Contains("=", output); // Should have KEY=VALUE format

        // Verify at least some variables are present
        var lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length > 0);
    }

    [Fact]
    public void Env_WithSingleVariable_DisplaysThatVariable()
    {
        // Set a test environment variable
        Environment.SetEnvironmentVariable("TEST_ENV_VAR", "test_value", EnvironmentVariableTarget.Process);

        var cmd = new EnvCommand();
        cmd.Execute(new[] { "TEST_ENV_VAR" });

        var output = _output.ToString();
        Assert.Contains("TEST_ENV_VAR=test_value", output);
    }

    [Fact]
    public void Env_WithMultipleVariables_DisplaysThoseVariables()
    {
        Environment.SetEnvironmentVariable("TEST_VAR1", "value1", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("TEST_VAR2", "value2", EnvironmentVariableTarget.Process);

        var cmd = new EnvCommand();
        cmd.Execute(new[] { "TEST_VAR1", "TEST_VAR2" });

        var output = _output.ToString();
        Assert.Contains("TEST_VAR1=value1", output);
        Assert.Contains("TEST_VAR2=value2", output);
    }

    [Fact]
    public void Env_WithNonexistentVariable_ProducesNoOutput()
    {
        var cmd = new EnvCommand();
        cmd.Execute(new[] { "NONEXISTENT_VAR_XYZ_12345" });

        var output = _output.ToString();
        Assert.DoesNotContain("NONEXISTENT_VAR_XYZ_12345", output);
    }

    [Fact]
    public void Env_WithMixedExistingAndNonexisting_DisplaysOnlyExisting()
    {
        Environment.SetEnvironmentVariable("TEST_EXISTS", "exists", EnvironmentVariableTarget.Process);

        var cmd = new EnvCommand();
        cmd.Execute(new[] { "TEST_EXISTS", "DOES_NOT_EXIST" });

        var output = _output.ToString();
        Assert.Contains("TEST_EXISTS=exists", output);
        Assert.DoesNotContain("DOES_NOT_EXIST", output);
    }

    [Fact]
    public void Env_DisplaysPathVariable()
    {
        string envPath = Environment.GetEnvironmentVariable("PATH");
        if (envPath == null)
        {
            envPath = string.Empty;
            Environment.SetEnvironmentVariable("PATH", envPath, EnvironmentVariableTarget.Process);
        }
        var cmd = new EnvCommand();
        cmd.Execute(new[] { "PATH" });

        var output = _output.ToString();
        // Check case-insensitively since PATH might be stored as "Path" or "PATH"
        Assert.Contains("PATH=", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Env_WithEmptyValue_DisplaysCorrectly()
    {
        Environment.SetEnvironmentVariable("EMPTY_VAR", "", EnvironmentVariableTarget.Process);

        var cmd = new EnvCommand();
        cmd.Execute(new[] { "EMPTY_VAR" });

        var output = _output.ToString();
        Assert.Contains("EMPTY_VAR=", output);
    }

    [Fact]
    public void Env_ReturnsTrue()
    {
        var cmd = new EnvCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void Env_WithSpecialCharactersInValue_DisplaysCorrectly()
    {
        Environment.SetEnvironmentVariable("SPECIAL_VAR", "value with spaces & special !@#", EnvironmentVariableTarget.Process);

        var cmd = new EnvCommand();
        cmd.Execute(new[] { "SPECIAL_VAR" });

        var output = _output.ToString();
        Assert.Contains("SPECIAL_VAR=value with spaces & special !@#", output);
    }
}
