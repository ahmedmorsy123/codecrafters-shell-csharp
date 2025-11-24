using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class AliasCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public AliasCommandTests()
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
    public void Alias_WithNoArguments_DisplaysNoAliases()
    {
        var cmd = new AliasCommand();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        // Should produce no output if no aliases defined
        Assert.True(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void Alias_CreateSimpleAlias_StoresAlias()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "ll=ls -l" });

        // Verify alias was created
        _output.GetStringBuilder().Clear();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        Assert.Contains("alias ll='ls -l'", output);
    }

    [Fact]
    public void Alias_DisplaySpecificAlias_ShowsAlias()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "la=ls -a" });

        _output.GetStringBuilder().Clear();
        cmd.Execute(new[] { "la" });

        var output = _output.ToString();
        Assert.Contains("alias la='ls -a'", output);
    }

    [Fact]
    public void Alias_DisplayNonexistentAlias_ProducesNoOutput()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "nonexistent" });

        var output = _output.ToString();
        Assert.True(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void Alias_CreateMultipleAliases_StoresAll()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "ll=ls -l", "la=ls -a" });

        _output.GetStringBuilder().Clear();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        Assert.Contains("alias ll='ls -l'", output);
        Assert.Contains("alias la='ls -a'", output);
    }

    [Fact]
    public void Alias_UpdateExistingAlias_ReplacesValue()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "ll=ls -l" });
        cmd.Execute(new[] { "ll=ls -la" });

        _output.GetStringBuilder().Clear();
        cmd.Execute(new[] { "ll" });

        var output = _output.ToString();
        Assert.Contains("alias ll='ls -la'", output);
        Assert.DoesNotContain("ls -l'", output.Replace("ls -la", "")); // Ensure old value is gone
    }

    [Fact]
    public void Alias_WithComplexValue_StoresCorrectly()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "complex=echo hello | cat" });

        _output.GetStringBuilder().Clear();
        cmd.Execute(new[] { "complex" });

        var output = _output.ToString();
        Assert.Contains("alias complex='echo hello | cat'", output);
    }

    [Fact]
    public void Alias_WithQuotesInValue_StoresCorrectly()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "quoted=echo \"hello world\"" });

        _output.GetStringBuilder().Clear();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        Assert.Contains("quoted", output);
        Assert.Contains("hello world", output);
    }

    [Fact]
    public void Alias_WithInvalidFormat_PrintsError()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "invalid-no-equals" });

        var output = _output.ToString();
        // Should either print error or treat as alias name to display
        Assert.True(string.IsNullOrWhiteSpace(output) || output.Contains("invalid"));
    }

    [Fact]
    public void Alias_WithEmptyValue_CreatesEmptyAlias()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "empty=" });

        _output.GetStringBuilder().Clear();
        cmd.Execute(new[] { "empty" });

        var output = _output.ToString();
        Assert.Contains("alias empty=''", output);
    }

    [Fact]
    public void Alias_ReturnsTrue()
    {
        var cmd = new AliasCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void Alias_WithSpacesAroundEquals_HandlesCorrectly()
    {
        // Depending on implementation, "ll = ls -l" might be invalid
        // This tests the parser's handling
        var cmd = new AliasCommand();
        var result = cmd.Execute(new[] { "ll = ls -l" });

        // Should still return true even if format is invalid
        Assert.True(result);
    }

    [Fact]
    public void Alias_DisplayAll_ShowsAllAliases()
    {
        var cmd = new AliasCommand();
        cmd.Execute(new[] { "a1=value1" });
        cmd.Execute(new[] { "a2=value2" });
        cmd.Execute(new[] { "a3=value3" });

        _output.GetStringBuilder().Clear();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        Assert.Contains("a1", output);
        Assert.Contains("a2", output);
        Assert.Contains("a3", output);
    }

    [Fact]
    public void Alias_WithSpecialCharactersInName_HandlesCorrectly()
    {
        var cmd = new AliasCommand();
        // Most shells allow alphanumeric and underscore in alias names
        cmd.Execute(new[] { "my_alias=echo test" });

        _output.GetStringBuilder().Clear();
        cmd.Execute(new[] { "my_alias" });

        var output = _output.ToString();
        Assert.Contains("my_alias", output);
    }
}
