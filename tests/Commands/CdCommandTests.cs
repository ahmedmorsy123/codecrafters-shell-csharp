using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class CdCommandTests : IDisposable
{
#nullable enable
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;
    private string? _tempDir;

    public CdCommandTests()
    {
        _output = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_output);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _output.Dispose();

        if (_tempDir != null && Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, true); } catch { }
        }
    }

    [Fact]
    public void Cd_ChangesDirectory()
    {
        var originalDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var cmd = new CdCommand();
            cmd.Execute(new[] { tempDir });

            Assert.Equal(Path.GetFullPath(tempDir).TrimEnd(Path.DirectorySeparatorChar),
                        Directory.GetCurrentDirectory().TrimEnd(Path.DirectorySeparatorChar));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir);
        }
    }

    [Fact]
    public void CdCommand_WithTilde_ExpandsHomeDirectory()
    {
        var homeDir = Environment.GetEnvironmentVariable("HOME")
                   ?? Environment.GetEnvironmentVariable("USERPROFILE");

        if (homeDir != null)
        {
            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                var cmd = new CdCommand();
                cmd.Execute(new[] { "~" });
                Assert.Equal(homeDir, Directory.GetCurrentDirectory());
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }
    }

    [Fact]
    public void CdCommand_WithRelativePath_ChangesDirectory()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        var originalDir = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(_tempDir);
            var subDir = Path.Combine(_tempDir, "subdir");
            Directory.CreateDirectory(subDir);

            var cmd = new CdCommand();
            cmd.Execute(new[] { "subdir" });
            Assert.Equal(subDir, Directory.GetCurrentDirectory());
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void CdCommand_WithNonexistentDirectory_PrintsError()
    {
        var cmd = new CdCommand();
        cmd.Execute(new[] { "/nonexistent/path/that/does/not/exist" });
        Assert.Contains("No such file or directory", _output.ToString());
    }

    [Fact]
    public void CdCommand_WithoutArgument_PrintsError()
    {
        var cmd = new CdCommand();
        cmd.Execute(Array.Empty<string>());
        Assert.Contains("missing argument", _output.ToString());
    }

    [Fact]
    public void CdCommand_WithTildeAndSubpath_ExpandsCorrectly()
    {
        var homeDir = Environment.GetEnvironmentVariable("HOME")
                   ?? Environment.GetEnvironmentVariable("USERPROFILE");

        if (homeDir != null)
        {
            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                var cmd = new CdCommand();
                cmd.Execute(new[] { "~/Documents" });
                var expectedPath = Path.Combine(homeDir, "Documents");
                // Check if directory exists first
                if (Directory.Exists(expectedPath))
                {
                    Assert.Equal(expectedPath, Directory.GetCurrentDirectory());
                }
                else
                {
                    Assert.Contains("No such file or directory", _output.ToString());
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }
    }

    [Fact]
    public void CdCommand_WithParentDirectory_ChangesToParent()
    {
        var originalDir = Directory.GetCurrentDirectory();
        var parentDir = Directory.GetParent(originalDir)?.FullName;

        if (parentDir != null)
        {
            try
            {
                var cmd = new CdCommand();
                cmd.Execute(new[] { ".." });
                Assert.Equal(parentDir, Directory.GetCurrentDirectory());
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }
    }

    [Fact]
    public void CdCommand_WithCurrentDirectory_StaysInPlace()
    {
        var originalDir = Directory.GetCurrentDirectory();

        try
        {
            var cmd = new CdCommand();
            cmd.Execute(new[] { "." });
            Assert.Equal(originalDir, Directory.GetCurrentDirectory());
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void CdCommand_WithAbsolutePath_ChangesDirectory()
    {
        var tempDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        var originalDir = Directory.GetCurrentDirectory();

        try
        {
            var cmd = new CdCommand();
            cmd.Execute(new[] { tempDir });
            Assert.Equal(tempDir, Directory.GetCurrentDirectory().TrimEnd(Path.DirectorySeparatorChar));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void CdCommand_WithInvalidPath_PrintsErrorMessage()
    {
        var cmd = new CdCommand();
        cmd.Execute(new[] { "!@#$%^&*()_+invalid_path_xyz" });
        var output = _output.ToString();
        Assert.True(output.Contains("No such file or directory") || output.Contains("error") || output.Length > 0);
    }

    [Fact]
    public void CdCommand_ReturnsTrue()
    {
        var cmd = new CdCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result); // Should always continue shell execution
    }
}
