using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Tests.Core;

public class HistoryPersistenceTests : IDisposable
{
#nullable enable
    private string? _tempHistoryFile;
    private string? _originalHistFile;

    public HistoryPersistenceTests()
    {
        PipelineHistory.ClearHistory();
        _originalHistFile = Environment.GetEnvironmentVariable("HISTFILE");
    }

    public void Dispose()
    {
        PipelineHistory.ClearHistory();

        // Restore original HISTFILE
        if (_originalHistFile != null)
            Environment.SetEnvironmentVariable("HISTFILE", _originalHistFile);
        else
            Environment.SetEnvironmentVariable("HISTFILE", null);

        // Clean up temp file
        if (_tempHistoryFile != null && File.Exists(_tempHistoryFile))
        {
            try { File.Delete(_tempHistoryFile); } catch { }
        }
    }

    [Fact]
    public void SaveHistoryToFile_CreatesFileWithCommands()
    {
        // Arrange
        _tempHistoryFile = Path.GetTempFileName();
        Environment.SetEnvironmentVariable("HISTFILE", _tempHistoryFile);

        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "test1" }) }));
        PipelineHistory.Add(new Pipeline(new[] { new Command("pwd", Array.Empty<string>()) }));
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "test2" }) }));

        // Act
        PipelineHistory.SaveHistoryToFile();

        // Assert
        Assert.True(File.Exists(_tempHistoryFile));
        var lines = File.ReadAllLines(_tempHistoryFile);
        Assert.Equal(3, lines.Length);
        Assert.Equal("echo test1", lines[0]);
        Assert.Equal("pwd", lines[1]);
        Assert.Equal("echo test2", lines[2]);
    }

    [Fact]
    public void LoadHistoryFromFile_ReadsCommandsIntoHistory()
    {
        // Arrange
        _tempHistoryFile = Path.GetTempFileName();
        File.WriteAllLines(_tempHistoryFile, new[]
        {
            "echo hello",
            "pwd",
            "cd /tmp"
        });
        Environment.SetEnvironmentVariable("HISTFILE", _tempHistoryFile);

        // Act
        PipelineHistory.LoadHistoryFromFile();

        // Assert
        var history = PipelineHistory.ListHistory().ToList();
        Assert.Equal(3, history.Count);
        Assert.Equal("echo hello", history[0].entry);
        Assert.Equal("pwd", history[1].entry);
        Assert.Equal("cd /tmp", history[2].entry);
    }

    [Fact]
    public void LoadHistoryFromFile_WithPipeline_LoadsCorrectly()
    {
        // Arrange
        _tempHistoryFile = Path.GetTempFileName();
        File.WriteAllLines(_tempHistoryFile, new[]
        {
            "echo hello | grep h",
            "cat file | wc -l"
        });
        Environment.SetEnvironmentVariable("HISTFILE", _tempHistoryFile);

        // Act
        PipelineHistory.LoadHistoryFromFile();

        // Assert
        var history = PipelineHistory.ListHistory().ToList();
        Assert.Equal(2, history.Count);
        Assert.Equal("echo hello | grep h", history[0].entry);
        Assert.Equal("cat file | wc -l", history[1].entry);
    }

    [Fact]
    public void LoadHistoryFromFile_FileDoesNotExist_DoesNotThrow()
    {
        // Arrange
        Environment.SetEnvironmentVariable("HISTFILE", "/nonexistent/path/history.txt");

        // Act & Assert
        var exception = Record.Exception(() => PipelineHistory.LoadHistoryFromFile());
        Assert.Null(exception);
    }

    [Fact]
    public void LoadHistoryFromFile_NoHISTFILE_DoesNothing()
    {
        // Arrange
        Environment.SetEnvironmentVariable("HISTFILE", null);

        // Act
        PipelineHistory.LoadHistoryFromFile();

        // Assert
        var history = PipelineHistory.ListHistory().ToList();
        Assert.Empty(history);
    }

    [Fact]
    public void SaveHistoryToFile_NoHISTFILE_DoesNothing()
    {
        // Arrange
        Environment.SetEnvironmentVariable("HISTFILE", null);
        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "test" }) }));

        // Act & Assert
        var exception = Record.Exception(() => PipelineHistory.SaveHistoryToFile());
        Assert.Null(exception);
    }

    [Fact]
    public void LoadHistoryFromFile_WithEmptyLines_SkipsEmptyLines()
    {
        // Arrange
        _tempHistoryFile = Path.GetTempFileName();
        File.WriteAllLines(_tempHistoryFile, new[]
        {
            "echo hello",
            "",
            "   ",
            "pwd"
        });
        Environment.SetEnvironmentVariable("HISTFILE", _tempHistoryFile);

        // Act
        PipelineHistory.LoadHistoryFromFile();

        // Assert
        var history = PipelineHistory.ListHistory().ToList();
        Assert.Equal(2, history.Count);
        Assert.Equal("echo hello", history[0].entry);
        Assert.Equal("pwd", history[1].entry);
    }

    [Fact]
    public void SaveHistoryToFile_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _tempHistoryFile = Path.Combine(tempDir, "history.txt");
        Environment.SetEnvironmentVariable("HISTFILE", _tempHistoryFile);

        PipelineHistory.Add(new Pipeline(new[] { new Command("echo", new[] { "test" }) }));

        // Act
        PipelineHistory.SaveHistoryToFile();

        // Assert
        Assert.True(File.Exists(_tempHistoryFile));

        // Cleanup
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
    }

    [Fact]
    public void LoadHistoryFromFile_WithTildeExpansion_ExpandsHomePath()
    {
        // Arrange
        var homeDir = Environment.GetEnvironmentVariable("HOME")
            ?? Environment.GetEnvironmentVariable("USERPROFILE");

        if (homeDir != null)
        {
            var histDir = Path.Combine(homeDir, ".test_shell_history_" + Guid.NewGuid().ToString());
            _tempHistoryFile = histDir;

            File.WriteAllLines(_tempHistoryFile, new[] { "echo test" });

            // Set HISTFILE with tilde
            var relativePath = Path.GetFileName(_tempHistoryFile);
            Environment.SetEnvironmentVariable("HISTFILE", $"~/{relativePath}");

            // Act
            PipelineHistory.LoadHistoryFromFile();

            // Assert
            var history = PipelineHistory.ListHistory().ToList();
            Assert.Single(history);
            Assert.Equal("echo test", history[0].entry);
        }
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesHistory()
    {
        // Arrange
        _tempHistoryFile = Path.GetTempFileName();
        Environment.SetEnvironmentVariable("HISTFILE", _tempHistoryFile);

        var originalCommands = new[]
        {
            "echo hello world",
            "pwd",
            "cd /tmp",
            "ls -la | grep test"
        };

        foreach (var cmd in originalCommands)
        {
            // ParsePipeline automatically adds to history
            CommandParser.ParsePipeline(cmd);
        }

        // Act - Save
        PipelineHistory.SaveHistoryToFile();

        // Clear and reload
        PipelineHistory.ClearHistory();
        PipelineHistory.LoadHistoryFromFile();

        // Assert
        var loadedHistory = PipelineHistory.ListHistory().ToList();
        Assert.Equal(originalCommands.Length, loadedHistory.Count);

        for (int i = 0; i < originalCommands.Length; i++)
        {
            Assert.Equal(originalCommands[i], loadedHistory[i].entry);
        }
    }

    [Fact]
    public void LoadHistoryFromFile_WithRelativePath_ConvertsToAbsolute()
    {
        // Arrange
        _tempHistoryFile = "temp_history_" + Guid.NewGuid().ToString() + ".txt";
        var absolutePath = Path.GetFullPath(_tempHistoryFile);

        File.WriteAllLines(_tempHistoryFile, new[] { "echo relative" });
        Environment.SetEnvironmentVariable("HISTFILE", _tempHistoryFile);

        // Act
        PipelineHistory.LoadHistoryFromFile();

        // Assert
        var history = PipelineHistory.ListHistory().ToList();
        Assert.Single(history);
        Assert.Equal("echo relative", history[0].entry);

        // Cleanup
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
    }
}
