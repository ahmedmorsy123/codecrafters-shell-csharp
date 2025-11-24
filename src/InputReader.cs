using System;
using System.Text;

/// <summary>
/// Handles interactive input with tab autocomplete for builtin commands
/// </summary>
public class InputReader
{
    // Console abstraction – can be swapped for testing
    private static IConsole _console = new RealConsole();

    /// <summary>
    /// Allows injecting a custom IConsole (e.g., a mock for tests)
    /// </summary>
    public static void SetConsole(IConsole console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    /// <summary>
    /// Reads a line of input with tab autocomplete support for builtin commands
    /// </summary>
    public static string? ReadLine()
    {
        // Check if stdin is redirected (e.g., from a pipe or file)
        if (_console.IsInputRedirected)
        {
            return ReadRedirectedInput();
        }
        else
        {
            return ReadInteractiveInput();
        }
    }

    /// <summary>
    /// Reads input from redirected stdin (for automated tests)
    /// </summary>
    private static string? ReadRedirectedInput()
    {
        StringBuilder line = new StringBuilder();
        int ch;

        while ((ch = _console.Read()) != -1 && ch != '\n' && ch != '\r')
        {
            char c = (char)ch;

            if (c == '\t') // Tab character
            {
                CompletionResult result = Autocomplete.GetSuggestion(line.ToString());
                if (result.Matches.Count > 0 && result.Matches[0] != line.ToString())
                {
                    if (result.Matches.Count == 1)
                    {
                        string addedPart = result.Matches[0].Substring(line.Length);
                        string suffix = result.IsComplete ? " " : "";
                        line.Append(addedPart + suffix);
                        _console.Write(addedPart + suffix);
                    }
                    else
                    {
                        _console.Write('\x07'); // Beep
                        if (_console.Read() == '\t')
                        {
                            _console.WriteLine("");
                            foreach (var cmd in result.Matches)
                            {
                                _console.Write(cmd + "  ");
                            }
                            // Redraw prompt and current line
                            _console.WriteLine("\n$ " + line.ToString());
                        }
                    }
                }
                else
                {
                    _console.Write('\x07'); // Beep
                }
            }
            else if (c == '\x1b') // ESC – start of ANSI sequence
            {
                int next1 = _console.Read();
                if (next1 == '[')
                {
                    int next2 = _console.Read();
                    if (next2 == 'A') // Up arrow
                    {
                        var previous = PipelineHistory.GetPrevious();
                        if (previous != null)
                        {
                            line.Clear();
                            line.Append(previous.ToString());
                            _console.Write("\r$ " + line.ToString());
                        }
                    }
                    else if (next2 == 'B') // Down arrow
                    {
                        var next = PipelineHistory.GetNext();
                        if (next != null)
                        {
                            line.Clear();
                            line.Append(next.ToString());
                            _console.Write("\r$ " + line.ToString());
                        }
                        else
                        {
                            line.Clear();
                            _console.Write("\r$ ");
                        }
                    }
                }
            }
            else
            {
                line.Append(c);
                _console.Write(c);
            }
        }

        // EOF with no characters -> null
        if (ch == -1 && line.Length == 0)
        {
            return null;
        }

        // Handle Windows \r\n vs Unix \n
        if (ch == '\r')
        {
            _ = _console.Read(); // consume \n if present
        }

        return line.ToString();
    }

    /// <summary>
    /// Reads input interactively with simple Tab completion
    /// </summary>
    private static string ReadInteractiveInput()
    {
        StringBuilder line = new StringBuilder();
        int cursorPosition = 0;

        while (true)
        {
            ConsoleKeyInfo keyInfo = _console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                _console.WriteLine("");
                return line.ToString();
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                var previous = PipelineHistory.GetPrevious();
                if (previous != null)
                {
                    ConsoleHelper.ClearLine(line.ToString(), cursorPosition);
                    line.Clear();
                    line.Append(previous.ToString());
                    cursorPosition = line.Length;
                    _console.Write(line.ToString());
                }
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                var next = PipelineHistory.GetNext();
                if (next != null)
                {
                    ConsoleHelper.ClearLine(line.ToString(), cursorPosition);
                    line.Clear();
                    line.Append(next.ToString());
                    cursorPosition = line.Length;
                    _console.Write(line.ToString());
                }
                else
                {
                    ConsoleHelper.ClearLine(line.ToString(), cursorPosition);
                    line.Clear();
                    cursorPosition = 0;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Tab)
            {
                if (cursorPosition == line.Length)
                {
                    var result = Autocomplete.GetSuggestion(line.ToString());
                    if (result.Matches.Count > 0 && result.Matches[0] != line.ToString())
                    {
                        if (result.Matches.Count == 1)
                        {
                            string added = result.Matches[0].Substring(line.Length);
                            string suffix = result.IsComplete ? " " : "";
                            line.Append(added + suffix);
                            cursorPosition = line.Length;
                            _console.Write(added + suffix);
                        }
                        else
                        {
                            _console.Write('\x07');
                            if (_console.ReadKey(intercept: true).Key == ConsoleKey.Tab)
                            {
                                _console.WriteLine("");
                                foreach (var cmd in result.Matches)
                                    _console.Write(cmd + "  ");
                                _console.Write("\n$ " + line.ToString());
                            }
                        }
                    }
                    else
                    {
                        _console.Write('\x07');
                    }
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (cursorPosition > 0)
                {
                    line.Remove(cursorPosition - 1, 1);
                    cursorPosition--;
                    // Redraw from cursor
                    _console.Write("\b");
                    _console.Write(line.ToString().Substring(cursorPosition) + " ");
                    _console.Write(new string('\b', line.Length - cursorPosition + 1));
                }
            }
            else if (keyInfo.Key == ConsoleKey.Delete)
            {
                if (cursorPosition < line.Length)
                {
                    line.Remove(cursorPosition, 1);
                    _console.Write(line.ToString().Substring(cursorPosition) + " ");
                    _console.Write(new string('\b', line.Length - cursorPosition + 1));
                }
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                if (cursorPosition > 0)
                {
                    cursorPosition--;
                    _console.Write("\b");
                }
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                if (cursorPosition < line.Length)
                {
                    _console.Write(line[cursorPosition].ToString());
                    cursorPosition++;
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                line.Insert(cursorPosition, keyInfo.KeyChar);
                cursorPosition++;
                // Redraw from insertion point
                _console.Write(line.ToString().Substring(cursorPosition - 1));
                _console.Write(new string('\b', line.Length - cursorPosition));
            }
        }
    }
}
