using System;
using System.Linq;
using Xunit;

namespace Tests.Core;

public class PipelineHistoryTests : IDisposable
{
    public PipelineHistoryTests()
    {
        PipelineHistory.ClearHistory();
    }

    public void Dispose()
    {
        PipelineHistory.ClearHistory();
    }

    [Fact]
    public void Add_AddsPipelineToHistory()
    {
        var pipeline = CommandParser.ParsePipeline("echo hello");

        var history = PipelineHistory.ListHistory();
        Assert.Single(history);
    }

    [Fact]
    public void GetPrevious_ReturnsLastCommand()
    {
        PipelineHistory.ClearHistory();
        var pipeline1 = new Pipeline(new[] { new Command("echo", new[] { "first" }) });
        var pipeline2 = new Pipeline(new[] { new Command("echo", new[] { "second" }) });

        PipelineHistory.Add(pipeline1);
        PipelineHistory.Add(pipeline2);

        var result = PipelineHistory.GetPrevious();
        Assert.NotNull(result);
        Assert.Equal("echo second", result.ToString());
    }

    [Fact]
    public void GetPrevious_CalledTwice_ReturnsOlderCommand()
    {
        PipelineHistory.ClearHistory();
        var pipeline1 = new Pipeline(new[] { new Command("echo", new[] { "first" }) });
        var pipeline2 = new Pipeline(new[] { new Command("echo", new[] { "second" }) });

        PipelineHistory.Add(pipeline1);
        PipelineHistory.Add(pipeline2);

        PipelineHistory.GetPrevious(); // Get second
        var result = PipelineHistory.GetPrevious(); // Get first

        Assert.NotNull(result);
        Assert.Equal("echo first", result.ToString());
    }

    [Fact]
    public void GetNext_AfterPrevious_MovesForward()
    {
        PipelineHistory.ClearHistory();
        var pipeline1 = new Pipeline(new[] { new Command("echo", new[] { "first" }) });
        var pipeline2 = new Pipeline(new[] { new Command("echo", new[] { "second" }) });

        PipelineHistory.Add(pipeline1);
        PipelineHistory.Add(pipeline2);

        var prev1 = PipelineHistory.GetPrevious(); // Should get second (last)
        Assert.NotNull(prev1);
        Assert.Equal("echo second", prev1.ToString());

        var prev2 = PipelineHistory.GetPrevious(); // Should get first
        Assert.NotNull(prev2);
        Assert.Equal("echo first", prev2.ToString());

        var next = PipelineHistory.GetNext(); // Should move back to second
        Assert.NotNull(next);
        Assert.Equal("echo second", next.ToString());
    }
    [Fact]
    public void GetPrevious_EmptyHistory_ReturnsNull()
    {
        PipelineHistory.ClearHistory();
        var result = PipelineHistory.GetPrevious();
        Assert.Null(result);
    }

    [Fact]
    public void GetNext_AtEnd_ReturnsNull()
    {
        PipelineHistory.ClearHistory();
        var pipeline = new Pipeline(new[] { new Command("echo", new[] { "hello" }) });
        PipelineHistory.Add(pipeline);

        var result = PipelineHistory.GetNext();
        Assert.Null(result);
    }

    [Fact]
    public void ListHistory_ReturnsAllCommands()
    {
        PipelineHistory.ClearHistory();
        var pipeline1 = new Pipeline(new[] { new Command("echo", new[] { "first" }) });
        var pipeline2 = new Pipeline(new[] { new Command("echo", new[] { "second" }) });

        PipelineHistory.Add(pipeline1);
        PipelineHistory.Add(pipeline2);

        var history = PipelineHistory.ListHistory();
        var list = history.ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0].position);
        Assert.Equal(2, list[1].position);
    }

    [Fact]
    public void ClearHistory_RemovesAllHistory()
    {
        var pipeline = new Pipeline(new[] { new Command("echo", new[] { "hello" }) });
        PipelineHistory.Add(pipeline);

        PipelineHistory.ClearHistory();

        var history = PipelineHistory.ListHistory();
        Assert.Empty(history);
    }
}
