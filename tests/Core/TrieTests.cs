using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Core;

public class TrieTests
{
    [Fact]
    public void Insert_SingleWord_CanBeFound()
    {
        var trie = new Trie();
        trie.Insert("hello");

        var results = trie.FindAllWithPrefix("hel");
        Assert.Single(results);
        Assert.Equal("hello", results[0]);
    }

    [Fact]
    public void Insert_MultipleWords_AllFound()
    {
        var trie = new Trie();
        trie.Insert("hello");
        trie.Insert("help");
        trie.Insert("hero");

        var results = trie.FindAllWithPrefix("he");
        Assert.Equal(3, results.Count);
        Assert.Contains("hello", results);
        Assert.Contains("help", results);
        Assert.Contains("hero", results);
    }

    [Fact]
    public void FindAllWithPrefix_NoMatch_ReturnsEmpty()
    {
        var trie = new Trie();
        trie.Insert("hello");

        var results = trie.FindAllWithPrefix("xyz");
        Assert.Empty(results);
    }

    [Fact]
    public void FindAllWithPrefix_ExactMatch_ReturnsMatch()
    {
        var trie = new Trie();
        trie.Insert("hello");

        var results = trie.FindAllWithPrefix("hello");
        Assert.Single(results);
        Assert.Equal("hello", results[0]);
    }

    [Fact]
    public void FindAllWithPrefix_CaseInsensitive_ReturnsMatch()
    {
        var trie = new Trie();
        trie.Insert("Hello");

        var results = trie.FindAllWithPrefix("hel");
        Assert.Single(results);
        Assert.Equal("hello", results[0]);
    }

    [Fact]
    public void GetFirstMatch_ReturnsFirstMatch()
    {
        var trie = new Trie();
        trie.Insert("hello");
        trie.Insert("help");

        var result = trie.GetFirstMatch("hel");
        Assert.NotNull(result);
        Assert.StartsWith("hel", result);
    }

    [Fact]
    public void GetFirstMatch_NoMatch_ReturnsNull()
    {
        var trie = new Trie();
        trie.Insert("hello");

        var result = trie.GetFirstMatch("xyz");
        Assert.Null(result);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var trie = new Trie();
        trie.Insert("hello");
        trie.Insert("help");

        trie.Clear();

        var results = trie.FindAllWithPrefix("he");
        Assert.Empty(results);
    }

    [Fact]
    public void GetAllMatches_ReturnsAllMatches()
    {
        var trie = new Trie();
        trie.Insert("apple");
        trie.Insert("application");
        trie.Insert("apply");

        var results = trie.GetAllMatches("app");
        Assert.Equal(3, results.Count);
    }
}
