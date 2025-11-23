using System.Text;

/// <summary>
/// Handles interactive input with tab autocomplete for builtin commands
/// </summary>
public class InputReader
{
    /// <summary>
    /// Reads a line of input with tab autocomplete support for builtin commands
    /// </summary>
    public static string? ReadLine()
    {
        // Check if stdin is redirected (e.g., from a pipe or file)
        if (Console.IsInputRedirected)
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

        while ((ch = Console.Read()) != -1 && ch != '\n' && ch != '\r')
        {
            char c = (char)ch;

            if (c == '\t')  // Tab character
            {
                CompletionResult result = Autocomplete.GetSuggestion(line.ToString());
                if (result.Matches.Count > 0 && result.Matches[0] != line.ToString())
                {
                    if (result.Matches.Count == 1)
                    {
                        string addedPart = result.Matches[0].Substring(line.Length);
                        string suffix = result.IsComplete ? " " : "";
                        line.Append(addedPart + suffix);
                        Console.Write(addedPart + suffix);
                    }
                    else
                    {
                        Console.Write('\x07'); // Beep sound
                        if (Console.Read() == '\t')
                        {
                            Console.WriteLine();
                            foreach (var cmd in result.Matches)
                            {
                                Console.Write(cmd + "  ");
                            }
                            // Redraw the prompt and current line
                            Console.WriteLine("\n$ " + line.ToString());
                        }
                    }
                }
                else
                {
                    Console.Write('\x07'); // Beep sound
                }
            }
            else
            {
                line.Append(c);
                Console.Write(c);
            }
        }

        // If we reached EOF before reading any characters, signal EOF
        if (ch == -1 && line.Length == 0)
        {
            return null;
        }

        // Handle \r\n (Windows) vs \n (Unix)
        if (ch == '\r')
        {
            int next = Console.Read();
            // Just consume the \n if it exists
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
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return line.ToString();
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                // Get previous command from history
                Pipeline? previous = PipelineHistory.GetPrevious();
                if (previous != null)
                {
                    // Clear current line
                    ClearCurrentLine(line.ToString(), cursorPosition);

                    // Set new line from history
                    line.Clear();
                    line.Append(previous.ToString());
                    cursorPosition = line.Length;
                    Console.Write(line.ToString());
                }
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                // Get next command from history
                Pipeline? next = PipelineHistory.GetNext();
                if (next != null)
                {
                    // Clear current line
                    ClearCurrentLine(line.ToString(), cursorPosition);

                    // Set new line from history
                    line.Clear();
                    line.Append(next.ToString());
                    cursorPosition = line.Length;
                    Console.Write(line.ToString());
                }
                else
                {
                    // At the end of history, clear the line
                    ClearCurrentLine(line.ToString(), cursorPosition);
                    line.Clear();
                    cursorPosition = 0;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Tab)
            {
                // Only autocomplete when cursor is at the end
                if (cursorPosition == line.Length)
                {
                    CompletionResult result = Autocomplete.GetSuggestion(line.ToString());
                    if (result.Matches.Count > 0 && result.Matches[0] != line.ToString())
                    {
                        if (result.Matches.Count == 1)
                        {
                            string addedPart = result.Matches[0].Substring(line.Length);
                            string suffix = result.IsComplete ? " " : "";
                            line.Append(addedPart + suffix);
                            cursorPosition = line.Length;
                            Console.Write(addedPart + suffix);
                        }
                        else
                        {
                            Console.Write('\x07'); // Beep sound
                            if (Console.ReadKey(intercept: true).Key == ConsoleKey.Tab)
                            {
                                Console.WriteLine();
                                foreach (var cmd in result.Matches)
                                {
                                    Console.Write(cmd + "  ");
                                }
                                // Redraw the prompt and current line
                                Console.Write("\n$ " + line.ToString());
                            }
                        }

                    }
                    else
                    {
                        Console.Write('\x07'); // Beep sound
                    }
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (cursorPosition > 0)
                {
                    line.Remove(cursorPosition - 1, 1);
                    cursorPosition--;

                    // Redraw from cursor to end
                    Console.Write("\b");
                    Console.Write(line.ToString().Substring(cursorPosition) + " ");
                    Console.Write(new string('\b', line.Length - cursorPosition + 1));
                }
            }
            else if (keyInfo.Key == ConsoleKey.Delete)
            {
                if (cursorPosition < line.Length)
                {
                    line.Remove(cursorPosition, 1);

                    // Redraw from cursor to end
                    Console.Write(line.ToString().Substring(cursorPosition) + " ");
                    Console.Write(new string('\b', line.Length - cursorPosition + 1));
                }
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                if (cursorPosition > 0)
                {
                    cursorPosition--;
                    Console.Write("\b");
                }
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                if (cursorPosition < line.Length)
                {
                    Console.Write(line[cursorPosition]);
                    cursorPosition++;
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                // Insert character at cursor position
                line.Insert(cursorPosition, keyInfo.KeyChar);
                cursorPosition++;

                // Redraw from cursor to end
                Console.Write(line.ToString().Substring(cursorPosition - 1));
                Console.Write(new string('\b', line.Length - cursorPosition));
            }
        }
    }

    /// <summary>
    /// Clears the current line in the console
    /// </summary>
    private static void ClearCurrentLine(string currentText, int cursorPosition)
    {
        // Move cursor to the beginning of the line
        Console.Write(new string('\b', cursorPosition));
        // Overwrite the entire line with spaces
        Console.Write(new string(' ', currentText.Length));
        // Move cursor back to the beginning
        Console.Write(new string('\b', currentText.Length));
    }
}
