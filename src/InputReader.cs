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
                List<string> completions = Autocomplete.GetSuggestion(line.ToString());
                if (completions.Count > 0 && completions[0] != line.ToString())
                {
                    if (completions.Count == 1)
                    {
                        string addedPart = completions[0].Substring(line.Length);
                        line.Append(addedPart + " ");
                        Console.Write(addedPart + " ");
                    }
                    else
                    {
                        Console.Write('\x07'); // Beep sound
                        if (Console.Read() == '\t')
                        {
                            Console.WriteLine();
                            foreach (var cmd in completions)
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
            else if (keyInfo.Key == ConsoleKey.Tab)
            {
                // Only autocomplete when cursor is at the end
                if (cursorPosition == line.Length)
                {
                    List<string> completions = Autocomplete.GetSuggestion(line.ToString());
                    if (completions.Count > 0 && completions[0] != line.ToString())
                    {
                        if (completions.Count == 1)
                        {
                            string addedPart = completions[0].Substring(line.Length);
                            line.Append(addedPart + " ");
                            cursorPosition = line.Length;
                            Console.Write(addedPart + " ");
                        }
                        else
                        {
                            Console.Write('\x07'); // Beep sound
                            if (Console.ReadKey(intercept: true).Key == ConsoleKey.Tab)
                            {
                                Console.WriteLine();
                                foreach (var cmd in completions)
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
}
