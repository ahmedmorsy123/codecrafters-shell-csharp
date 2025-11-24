using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class CatCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public CatCommandTests()
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
    public void Cat_WithSingleFile_DisplaysContents()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "Hello, World!");

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile });

            var output = _output.ToString();
            Assert.Contains("Hello, World!", output);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Cat_WithMultipleFiles_DisplaysAllContents()
    {
        var tempFile1 = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile1, "File 1 content");
            File.WriteAllText(tempFile2, "File 2 content");

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile1, tempFile2 });

            var output = _output.ToString();
            Assert.Contains("File 1 content", output);
            Assert.Contains("File 2 content", output);
        }
        finally
        {
            if (File.Exists(tempFile1)) File.Delete(tempFile1);
            if (File.Exists(tempFile2)) File.Delete(tempFile2);
        }
    }

    [Fact]
    public void Cat_WithNonexistentFile_PrintsError()
    {
        var cmd = new CatCommand();
        cmd.Execute(new[] { "/nonexistent/file.txt" });

        var output = _output.ToString();
        Assert.Contains("cat:", output);
        Assert.Contains("No such file or directory", output);
    }

    [Fact]
    public void Cat_WithMixedExistingAndNonexisting_DisplaysExistingAndErrors()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "Exists");

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile, "/nonexistent.txt" });

            var output = _output.ToString();
            Assert.Contains("Exists", output);
            Assert.Contains("No such file or directory", output);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Cat_WithEmptyFile_ProducesNoOutput()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "");

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile });

            var output = _output.ToString();
            Assert.True(string.IsNullOrEmpty(output));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Cat_WithMultilineFile_DisplaysAllLines()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "Line 1\nLine 2\nLine 3");

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile });

            var output = _output.ToString();
            Assert.Contains("Line 1", output);
            Assert.Contains("Line 2", output);
            Assert.Contains("Line 3", output);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Cat_WithSpecialCharacters_DisplaysCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "Special: !@#$%^&*()");

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile });

            var output = _output.ToString();
            Assert.Contains("Special: !@#$%^&*()", output);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Cat_ReturnsTrue()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test");

            var cmd = new CatCommand();
            var result = cmd.Execute(new[] { tempFile });
            Assert.True(result);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Cat_WithErrorStillReturnsTrue()
    {
        var cmd = new CatCommand();
        var result = cmd.Execute(new[] { "/nonexistent.txt" });
        Assert.True(result);
    }

    [Fact]
    public void Cat_PreservesWhitespace()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var content = "  indented\n\ttabbed\n  multiple  spaces";
            File.WriteAllText(tempFile, content);

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile });

            var output = _output.ToString();
            Assert.Contains("  indented", output);
            Assert.Contains("\ttabbed", output);
            Assert.Contains("  multiple  spaces", output);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Cat_WithLargeFile_DisplaysAllContent()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            // Create a file with many lines
            var lines = new string[100];
            for (int i = 0; i < 100; i++)
            {
                lines[i] = $"Line {i}";
            }
            File.WriteAllLines(tempFile, lines);

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile });

            var output = _output.ToString();
            Assert.Contains("Line 0", output);
            Assert.Contains("Line 50", output);
            Assert.Contains("Line 99", output);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Cat_ConcatenatesFilesInOrder()
    {
        var tempFile1 = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        var tempFile3 = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile1, "FIRST");
            File.WriteAllText(tempFile2, "SECOND");
            File.WriteAllText(tempFile3, "THIRD");

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile1, tempFile2, tempFile3 });

            var output = _output.ToString();
            var firstPos = output.IndexOf("FIRST");
            var secondPos = output.IndexOf("SECOND");
            var thirdPos = output.IndexOf("THIRD");

            Assert.True(firstPos < secondPos);
            Assert.True(secondPos < thirdPos);
        }
        finally
        {
            if (File.Exists(tempFile1)) File.Delete(tempFile1);
            if (File.Exists(tempFile2)) File.Delete(tempFile2);
            if (File.Exists(tempFile3)) File.Delete(tempFile3);
        }
    }

    [Fact]
    public void Cat_WithUnicodeContent_DisplaysCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "Hello 世界 🌍");

            var cmd = new CatCommand();
            cmd.Execute(new[] { tempFile });

            var output = _output.ToString();
            Assert.Contains("Hello 世界 🌍", output);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Cat_WithNoArguments_ReadsFromStdin()
    {
        var originalInput = Console.In;
        try
        {
            // Simulate stdin input
            var input = new StringReader("line1\nline2\nline3");
            Console.SetIn(input);

            var cmd = new CatCommand();
            cmd.Execute(Array.Empty<string>());

            var output = _output.ToString();
            Assert.Contains("line1", output);
            Assert.Contains("line2", output);
            Assert.Contains("line3", output);
        }
        finally
        {
            Console.SetIn(originalInput);
        }
    }
}
