using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Core;

public class InputReaderTests : IDisposable
{
    public InputReaderTests()
    {
        // Clear static state before each test
        PipelineHistory.ClearHistory();
        Autocomplete.Clear();
    }

    public void Dispose()
    {
        PipelineHistory.ClearHistory();
        Autocomplete.Clear();
    }

    [Fact]
    public void ReadLine_ReturnsTypedInput()
    {
        // Arrange
        var mock = new MockConsole();
        mock.EnqueueKey(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        // Act
        var result = InputReader.ReadLine();

        // Assert
        Assert.Equal("ls", result);
    }

    [Fact]
    public void HistoryNavigation_RecallsPreviousCommand()
    {
        // Arrange
        var mock = new MockConsole();

        // Add a command to history manually
        PipelineHistory.Add(CommandParser.ParsePipeline("echo hello"));

        // Simulate Up Arrow then Enter
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        // Act
        var result = InputReader.ReadLine();

        // Assert
        Assert.Equal("echo hello", result);
    }

    [Fact]
    public void TabCompletion_CompletesCommand()
    {
        // Arrange
        var mock = new MockConsole();
        Autocomplete.Register("echo");

        // Simulate typing "ec", pressing Tab, then Enter
        mock.EnqueueKey(new ConsoleKeyInfo('e', ConsoleKey.E, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('c', ConsoleKey.C, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        // Act
        var result = InputReader.ReadLine();

        // Assert
        Assert.Equal("echo ", result); // Expect space after completion
    }

    [Fact]
    public void TabCompletion_WithNoMatches_DoesNothing()
    {
        var mock = new MockConsole();
        Autocomplete.Register("echo");
        Autocomplete.Register("exit");

        // Type "xyz" (no match) and press Tab
        mock.EnqueueKey(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("xyz", result);
    }

    [Fact]
    public void TabCompletion_WithMultipleMatches_ShowsCommonPrefix()
    {
        var mock = new MockConsole();
        Autocomplete.Register("echo");
        Autocomplete.Register("exit");
        Autocomplete.Register("export");

        // Type "ex" and press Tab twice (double-tab shows all matches)
        mock.EnqueueKey(new ConsoleKeyInfo('e', ConsoleKey.E, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        // Should keep "ex" and possibly show matches
        Assert.StartsWith("ex", result);
    }

    [Fact]
    public void Backspace_AtBeginning_DoesNothing()
    {
        var mock = new MockConsole();

        // Press backspace at start, then type "ls"
        mock.EnqueueKey(new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("ls", result);
    }

    [Fact]
    public void Backspace_InMiddle_RemovesCharacter()
    {
        var mock = new MockConsole();

        // Type "lxs", backspace to remove 'x', then enter
        mock.EnqueueKey(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("ls", result);
    }

    [Fact]
    public void ArrowKeys_LeftRight_MoveCursor()
    {
        var mock = new MockConsole();

        // Type "ls", left, left, insert "x", result should be "xls"
        mock.EnqueueKey(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("xls", result);
    }

    [Fact]
    public void HistoryNavigation_UpDown_NavigatesHistory()
    {
        var mock = new MockConsole();

        PipelineHistory.Add(CommandParser.ParsePipeline("cmd1"));
        PipelineHistory.Add(CommandParser.ParsePipeline("cmd2"));

        // Up (cmd2), Up (cmd1), Down (cmd2), Enter
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("cmd2", result);
    }

    [Fact]
    public void HistoryNavigation_UpAtTop_StaysAtFirst()
    {
        var mock = new MockConsole();

        PipelineHistory.Add(CommandParser.ParsePipeline("only command"));

        // Multiple Ups should stay at first command
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("only command", result);
    }

    [Fact]
    public void HistoryNavigation_DownAtBottom_ReturnsToEmptyLine()
    {
        var mock = new MockConsole();

        PipelineHistory.Add(CommandParser.ParsePipeline("cmd1"));

        // Up (cmd1), Down (empty), type "new", Enter
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('n', ConsoleKey.N, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('e', ConsoleKey.E, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("new", result);
    }

    [Fact]
    public void Delete_RemovesCharacterAtCursor()
    {
        var mock = new MockConsole();

        // Type "lxs", left, delete (removes 's'), Enter
        mock.EnqueueKey(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("lx", result);
    }

    [Fact]
    public void EmptyInput_ReturnsEmptyString()
    {
        var mock = new MockConsole();

        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("", result);
    }

    [Fact]
    public void InsertCharacter_InMiddle_InsertsCorrectly()
    {
        var mock = new MockConsole();

        // Type "ac", left, insert "b" → "abc"
        mock.EnqueueKey(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('c', ConsoleKey.C, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('b', ConsoleKey.B, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("abc", result);
    }

    [Fact]
    public void ReadLine_WithRedirectedInput_ReturnsLine()
    {
        var mock = new MockConsole();
        mock.SetInputRedirected(true);
        mock.EnqueueChar('e');
        mock.EnqueueChar('c');
        mock.EnqueueChar('h');
        mock.EnqueueChar('o');
        mock.EnqueueChar('\n');
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("echo", result);
    }

    [Fact]
    public void ReadLine_WithRedirectedInput_EOF_ReturnsNull()
    {
        var mock = new MockConsole();
        mock.SetInputRedirected(true);
        mock.EnqueueChar(-1); // EOF
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Null(result);
    }

    [Fact]
    public void ReadLine_WithRedirectedInput_HandlesCarriageReturn()
    {
        var mock = new MockConsole();
        mock.SetInputRedirected(true);
        mock.EnqueueChar('l');
        mock.EnqueueChar('s');
        mock.EnqueueChar('\r');
        mock.EnqueueChar('\n');
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("ls", result);
    }

    [Fact]
    public void ReadLine_WithRedirectedInput_TabCompletion_SingleMatch()
    {
        var mock = new MockConsole();
        mock.SetInputRedirected(true);
        Autocomplete.Register("echo");

        mock.EnqueueChar('e');
        mock.EnqueueChar('c');
        mock.EnqueueChar('\t');
        mock.EnqueueChar('\n');
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.StartsWith("echo", result);
    }

    [Fact]
    public void ReadLine_WithRedirectedInput_TabCompletion_NoMatch()
    {
        var mock = new MockConsole();
        mock.SetInputRedirected(true);
        Autocomplete.Register("echo");

        mock.EnqueueChar('x');
        mock.EnqueueChar('y');
        mock.EnqueueChar('z');
        mock.EnqueueChar('\t');
        mock.EnqueueChar('\n');
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("xyz", result);
    }

    [Fact]
    public void ReadLine_WithRedirectedInput_TabCompletion_MultipleMatches()
    {
        var mock = new MockConsole();
        mock.SetInputRedirected(true);
        Autocomplete.Register("exit");
        Autocomplete.Register("export");

        mock.EnqueueChar('e');
        mock.EnqueueChar('x');
        mock.EnqueueChar('\t');
        mock.EnqueueChar('\t'); // Double tab shows list
        mock.EnqueueChar('\n');
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.StartsWith("ex", result);
    }

    [Fact]
    public void ReadLine_WithRedirectedInput_UpArrow_RecallsHistory()
    {
        var mock = new MockConsole();
        mock.SetInputRedirected(true);
        PipelineHistory.Add(CommandParser.ParsePipeline("previous"));

        mock.EnqueueChar('\x1b'); // ESC
        mock.EnqueueChar('[');
        mock.EnqueueChar('A'); // Up arrow
        mock.EnqueueChar('\n');
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("previous", result);
    }

    [Fact]
    public void ReadLine_WithRedirectedInput_DownArrow_ClearsLine()
    {
        var mock = new MockConsole();
        mock.SetInputRedirected(true);
        PipelineHistory.Add(CommandParser.ParsePipeline("cmd1"));

        mock.EnqueueChar('\x1b'); // ESC
        mock.EnqueueChar('[');
        mock.EnqueueChar('A'); // Up arrow (recall cmd1)
        mock.EnqueueChar('\x1b'); // ESC
        mock.EnqueueChar('[');
        mock.EnqueueChar('B'); // Down arrow (clear)
        mock.EnqueueChar('n');
        mock.EnqueueChar('e');
        mock.EnqueueChar('w');
        mock.EnqueueChar('\n');
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("new", result);
    }

    [Fact]
    public void ReadLine_WithRedirectedInput_IgnoresIncompleteEscapeSequence()
    {
        var mock = new MockConsole();
        mock.SetInputRedirected(true);

        mock.EnqueueChar('\x1b'); // ESC
        mock.EnqueueChar('x'); // Not '['
        mock.EnqueueChar('t');
        mock.EnqueueChar('e');
        mock.EnqueueChar('s');
        mock.EnqueueChar('t');
        mock.EnqueueChar('\n');
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        // ESC followed by non-'[' character 'x' is consumed, then "test" is read
        Assert.Equal("test", result);
    }
    [Fact]
    public void ReadLine_RightArrow_AtEnd_DoesNothing()
    {
        var mock = new MockConsole();

        mock.EnqueueKey(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("ls", result);
    }

    [Fact]
    public void ReadLine_LeftArrow_AtBeginning_DoesNothing()
    {
        var mock = new MockConsole();

        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("ls", result);
    }

    [Fact]
    public void ReadLine_Delete_AtEnd_DoesNothing()
    {
        var mock = new MockConsole();

        mock.EnqueueKey(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("ls", result);
    }

    [Fact]
    public void ReadLine_TabAtNonEnd_DoesNothing()
    {
        var mock = new MockConsole();
        Autocomplete.Register("echo");

        mock.EnqueueKey(new ConsoleKeyInfo('e', ConsoleKey.E, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('c', ConsoleKey.C, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));
        mock.EnqueueKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        InputReader.SetConsole(mock);

        var result = InputReader.ReadLine();
        Assert.Equal("ech", result);
    }
}

