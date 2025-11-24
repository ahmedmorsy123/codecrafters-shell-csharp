using System;
using Xunit;

namespace Tests.Core;

public class AutocompleteTests
{
    [Fact]
    public void GetSuggestion_SingleMatch_ReturnsCompleteMatch()
    {
        Autocomplete.Clear();
        Autocomplete.Register("echo");

        var result = Autocomplete.GetSuggestion("ec");

        Assert.Single(result.Matches);
        Assert.Equal("echo", result.Matches[0]);
        Assert.True(result.IsComplete);
    }

    [Fact]
    public void GetSuggestion_MultipleMatches_ReturnsAll()
    {
        Autocomplete.Clear();
        Autocomplete.Register("echo");
        Autocomplete.Register("exit");

        var result = Autocomplete.GetSuggestion("e");

        Assert.Equal(2, result.Matches.Count);
        Assert.Contains("echo", result.Matches);
        Assert.Contains("exit", result.Matches);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public void GetSuggestion_NoMatch_ReturnsEmpty()
    {
        Autocomplete.Clear();
        Autocomplete.Register("echo");

        var result = Autocomplete.GetSuggestion("xyz");

        Assert.Empty(result.Matches);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public void GetSuggestion_ExactMatch_ReturnsAsComplete()
    {
        Autocomplete.Clear();
        Autocomplete.Register("echo");

        var result = Autocomplete.GetSuggestion("echo");

        Assert.Single(result.Matches);
        Assert.True(result.IsComplete);
    }

    [Fact]
    public void Register_AddsCommandToAutocomplete()
    {
        Autocomplete.Clear();
        Autocomplete.Register("mycommand");

        var result = Autocomplete.GetSuggestion("myc");

        Assert.Single(result.Matches);
        Assert.Equal("mycommand", result.Matches[0]);
    }

    [Fact]
    public void Clear_RemovesAllCommands()
    {
        Autocomplete.Register("echo");
        Autocomplete.Register("exit");

        Autocomplete.Clear();

        var result = Autocomplete.GetSuggestion("e");
        Assert.Empty(result.Matches);
    }
}
