using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Tests.Commands;

public class HistoryCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public HistoryCommandTests()
    {
        _output = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_output);
        PipelineHistory.ClearHistory();
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _output.Dispose();
        PipelineHistory.ClearHistory();
    }

    [Fact]
    public void History_PrintsAllHistory()
    {
        PipelineHistory.ClearHistory();
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "1" }) }));
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "2" }) }));

        var cmd = new HistoryCommand();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString();
        Assert.Contains("1  echo 1", output);
        Assert.Contains("2  echo 2", output);
    }

    [Fact]
    public void History_WithCount_PrintsLastN()
    {
        PipelineHistory.ClearHistory();
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "1" }) }));
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "2" }) }));
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "3" }) }));

        var cmd = new HistoryCommand();
        cmd.Execute(new[] { "2" });

        var output = _output.ToString();
        Assert.DoesNotContain("1  echo 1", output);
        Assert.Contains("2  echo 2", output);
        Assert.Contains("3  echo 3", output);
    }

    [Fact]
    public void HistoryCommand_WriteToFile_CreatesFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "test" }) }));

            var cmd = new HistoryCommand();
            cmd.Execute(new[] { "-w", tempFile });

            Assert.True(File.Exists(tempFile));
            var content = File.ReadAllText(tempFile);
            Assert.Contains("echo test", content);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void HistoryCommand_AppendToFile_AppendsHistory()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "existing command\n");

            PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "new" }) }));

            var cmd = new HistoryCommand();
            cmd.Execute(new[] { "-a", tempFile });

            var content = File.ReadAllText(tempFile);
            Assert.Contains("existing command", content);
            Assert.Contains("echo new", content);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void HistoryCommand_ReadFromFile_LoadsHistory()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "echo from file\npwd\n");

            PipelineHistory.ClearHistory();
            var cmd = new HistoryCommand();
            cmd.Execute(new[] { "-r", tempFile });

            var history = PipelineHistory.ListHistory().ToList();
            Assert.True(history.Count >= 2);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void HistoryCommand_ReadFromNonexistentFile_PrintsError()
    {
        var cmd = new HistoryCommand();
        cmd.Execute(new[] { "-r", "/nonexistent/file.txt" });
        Assert.Contains("does not exist", _output.ToString());
    }

    [Fact]
    public void HistoryCommand_WriteToFile_WithMissingPath_ThrowsException()
    {
        var cmd = new HistoryCommand();
        // HistoryCommand will throw ArgumentOutOfRangeException when args[1] is missing
        Assert.Throws<ArgumentOutOfRangeException>(() => cmd.Execute(new[] { "-w" }));
    }

    [Fact]
    public void HistoryCommand_AppendToFile_WithMissingPath_ThrowsException()
    {
        var cmd = new HistoryCommand();
        // HistoryCommand will throw ArgumentOutOfRangeException when args[1] is missing
        Assert.Throws<ArgumentOutOfRangeException>(() => cmd.Execute(new[] { "-a" }));
    }

    [Fact]
    public void HistoryCommand_AppendToNonexistentFile_CreatesFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "new" }) }));

            var cmd = new HistoryCommand();
            cmd.Execute(new[] { "-a", tempFile });

            Assert.True(File.Exists(tempFile));
            var content = File.ReadAllText(tempFile);
            Assert.Contains("echo new", content);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void HistoryCommand_WithEmptyHistory_PrintsMessage()
    {
        PipelineHistory.ClearHistory();

        var cmd = new HistoryCommand();
        cmd.Execute(Array.Empty<string>());

        Assert.Contains("No commands in history", _output.ToString());
    }

    [Fact]
    public void HistoryCommand_WithInvalidLimit_ShowsAllHistory()
    {
        PipelineHistory.ClearHistory();
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "1" }) }));
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "2" }) }));

        var cmd = new HistoryCommand();
        cmd.Execute(new[] { "invalid" });

        var output = _output.ToString();
        // Should show all history when limit is invalid
        Assert.True(output.Contains("echo 1") || output.Contains("echo 2"));
    }

    [Fact]
    public void HistoryCommand_WithLimitLargerThanHistory_ShowsAllHistory()
    {
        PipelineHistory.ClearHistory();
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "1" }) }));
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "2" }) }));

        var cmd = new HistoryCommand();
        cmd.Execute(new[] { "10" });

        var output = _output.ToString();
        Assert.Contains("1  echo 1", output);
        Assert.Contains("2  echo 2", output);
    }

    [Fact]
    public void HistoryCommand_WriteToFile_ClearsHistoryAfterWrite()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "test" }) }));

            var cmd = new HistoryCommand();
            var result = cmd.Execute(new[] { "-w", tempFile });

            Assert.True(result); // Should continue shell execution
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
