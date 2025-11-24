using System;
using System.IO;
using Xunit;

namespace Tests.Core;

public class OutputRedirectorTests : IDisposable
{
#nullable enable
    private string? _tempFile;

    public void Dispose()
    {
        if (_tempFile != null && File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }

    [Fact]
    public void OutputRedirector_StdoutRedirection_WritesToFile()
    {
        // Arrange
        _tempFile = Path.GetTempFileName();
        var redirection = new RedirectionInfo
        {
            StdoutFile = _tempFile,
            StdoutAppend = false
        };

        // Act
        using (var redirector = new OutputRedirector(redirection))
        {
            Console.WriteLine("Test output");
        }

        // Assert
        var content = File.ReadAllText(_tempFile);
        Assert.Contains("Test output", content);
    }

    [Fact]
    public void OutputRedirector_StdoutAppend_AppendsToFile()
    {
        // Arrange
        _tempFile = Path.GetTempFileName();
        File.WriteAllText(_tempFile, "First line\n");

        var redirection = new RedirectionInfo
        {
            StdoutFile = _tempFile,
            StdoutAppend = true
        };

        // Act
        using (var redirector = new OutputRedirector(redirection))
        {
            Console.WriteLine("Second line");
        }

        // Assert
        var content = File.ReadAllText(_tempFile);
        Assert.Contains("First line", content);
        Assert.Contains("Second line", content);
    }

    [Fact]
    public void OutputRedirector_StdoutOverwrite_ReplacesFile()
    {
        // Arrange
        _tempFile = Path.GetTempFileName();
        File.WriteAllText(_tempFile, "Old content\n");

        var redirection = new RedirectionInfo
        {
            StdoutFile = _tempFile,
            StdoutAppend = false
        };

        // Act
        using (var redirector = new OutputRedirector(redirection))
        {
            Console.WriteLine("New content");
        }

        // Assert
        var content = File.ReadAllText(_tempFile);
        Assert.DoesNotContain("Old content", content);
        Assert.Contains("New content", content);
    }

    [Fact]
    public void OutputRedirector_StderrRedirection_WritesToErrorFile()
    {
        // Arrange
        _tempFile = Path.GetTempFileName();
        var redirection = new RedirectionInfo
        {
            StderrFile = _tempFile,
            StderrAppend = false
        };

        // Act
        using (var redirector = new OutputRedirector(redirection))
        {
            Console.Error.WriteLine("Error message");
        }

        // Assert
        var content = File.ReadAllText(_tempFile);
        Assert.Contains("Error message", content);
    }

    [Fact]
    public void OutputRedirector_NoRedirection_DoesNotAffectStreams()
    {
        // Arrange
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        var redirection = new RedirectionInfo();

        // Act
        using (var redirector = new OutputRedirector(redirection))
        {
            // Streams should remain unchanged
            Assert.Same(originalOut, Console.Out);
            Assert.Same(originalErr, Console.Error);
        }

        // Assert - streams restored
        Assert.Same(originalOut, Console.Out);
        Assert.Same(originalErr, Console.Error);
    }

    [Fact]
    public void OutputRedirector_Dispose_RestoresOriginalStreams()
    {
        // Arrange
        _tempFile = Path.GetTempFileName();
        var originalOut = Console.Out;
        var originalErr = Console.Error;

        var redirection = new RedirectionInfo
        {
            StdoutFile = _tempFile,
            StdoutAppend = false
        };

        // Act
        using (var redirector = new OutputRedirector(redirection))
        {
            // Streams should be redirected
            Assert.NotSame(originalOut, Console.Out);
        }

        // Assert - streams restored after disposal
        Assert.Same(originalOut, Console.Out);
        Assert.Same(originalErr, Console.Error);
    }
}
