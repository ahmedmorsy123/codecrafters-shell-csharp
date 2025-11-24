using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class UnaliasCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public UnaliasCommandTests()
    {
        _output = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_output);

        // Clear any existing aliases
        AliasManager.Clear();
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _output.Dispose();
        AliasManager.Clear();
    }

    [Fact]
    public void Unalias_RemovesSingleAlias()
    {
        // Create an alias first
        var aliasCmd = new AliasCommand();
        aliasCmd.Execute(new[] { "ll=ls -l" });

        // Remove it
        var unaliasCmd = new UnaliasCommand();
        unaliasCmd.Execute(new[] { "ll" });

        // Verify it's gone
        _output.GetStringBuilder().Clear();
        aliasCmd.Execute(new[] { "ll" });

        var output = _output.ToString();
        Assert.True(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void Unalias_RemoveMultipleAliases()
    {
        var aliasCmd = new AliasCommand();
        aliasCmd.Execute(new[] { "ll=ls -l", "la=ls -a" });

        var unaliasCmd = new UnaliasCommand();
        unaliasCmd.Execute(new[] { "ll", "la" });

        _output.GetStringBuilder().Clear();
        aliasCmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        Assert.True(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void Unalias_WithNonexistentAlias_PrintsError()
    {
        var cmd = new UnaliasCommand();
        cmd.Execute(new[] { "nonexistent" });

        var output = _output.ToString();
        Assert.Contains("not found", output);
        Assert.Contains("nonexistent", output);
    }

    [Fact]
    public void Unalias_WithNoArguments_PrintsError()
    {
        var cmd = new UnaliasCommand();
        cmd.Execute(Array.Empty<string>());

        Assert.Contains("missing argument", _output.ToString());
    }

    [Fact]
    public void Unalias_WithDashA_RemovesAllAliases()
    {
        var aliasCmd = new AliasCommand();
        aliasCmd.Execute(new[] { "a1=value1", "a2=value2", "a3=value3" });

        var unaliasCmd = new UnaliasCommand();
        unaliasCmd.Execute(new[] { "-a" });

        _output.GetStringBuilder().Clear();
        aliasCmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        Assert.True(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void Unalias_WithDashAIgnoresOtherArguments()
    {
        var aliasCmd = new AliasCommand();
        aliasCmd.Execute(new[] { "a1=value1", "a2=value2" });

        var unaliasCmd = new UnaliasCommand();
        unaliasCmd.Execute(new[] { "-a", "a1" });

        _output.GetStringBuilder().Clear();
        aliasCmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        // All aliases should be removed when -a is present
        Assert.True(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void Unalias_RemoveSomeAliases_LeavesOthers()
    {
        var aliasCmd = new AliasCommand();
        aliasCmd.Execute(new[] { "a1=value1", "a2=value2", "a3=value3" });

        var unaliasCmd = new UnaliasCommand();
        unaliasCmd.Execute(new[] { "a1", "a3" });

        _output.GetStringBuilder().Clear();
        aliasCmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        Assert.DoesNotContain("a1", output);
        Assert.Contains("a2", output);
        Assert.DoesNotContain("a3", output);
    }

    [Fact]
    public void Unalias_ReturnsTrue()
    {
        var cmd = new UnaliasCommand();
        var result = cmd.Execute(new[] { "-a" });
        Assert.True(result);
    }

    [Fact]
    public void Unalias_WithErrorStillReturnsTrue()
    {
        var cmd = new UnaliasCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void Unalias_WithMixedExistingAndNonexisting_RemovesExisting()
    {
        var aliasCmd = new AliasCommand();
        aliasCmd.Execute(new[] { "exists=value" });

        var unaliasCmd = new UnaliasCommand();
        unaliasCmd.Execute(new[] { "exists", "nonexistent" });

        var output = _output.ToString();
        // Should print error for nonexistent
        Assert.Contains("not found", output);
        Assert.Contains("nonexistent", output);

        // But should still remove the existing one
        _output.GetStringBuilder().Clear();
        aliasCmd.Execute(Array.Empty<string>());
        var afterOutput = _output.ToString();
        Assert.True(string.IsNullOrWhiteSpace(afterOutput));
    }

    [Fact]
    public void Unalias_CanRemoveAndRecreateAlias()
    {
        var aliasCmd = new AliasCommand();
        aliasCmd.Execute(new[] { "test=value1" });

        var unaliasCmd = new UnaliasCommand();
        unaliasCmd.Execute(new[] { "test" });

        // Recreate with different value
        aliasCmd.Execute(new[] { "test=value2" });

        _output.GetStringBuilder().Clear();
        aliasCmd.Execute(new[] { "test" });

        var output = _output.ToString();
        Assert.Contains("test='value2'", output);
        Assert.DoesNotContain("value1", output);
    }

    [Fact]
    public void Unalias_DashA_WorksWithEmptyAliases()
    {
        var cmd = new UnaliasCommand();
        var result = cmd.Execute(new[] { "-a" });

        Assert.True(result);
        // Should not produce error when no aliases exist
    }
}
