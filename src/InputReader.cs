using System.Text;

/// <summary>
/// Handles interactive input with tab autocomplete for builtin commands
/// </summary>
public class InputReader
{
    /// <summary>
    /// Reads a line of input with tab autocomplete support for builtin commands
    /// </summary>
    public static string ReadLine()
    {
        StringBuilder input = new StringBuilder();
        int cursorPosition = 0;
        int startColumn = Console.CursorLeft; // Remember where input starts (after "$ ")

        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                // Submit the command
                Console.WriteLine();
                return input.ToString();
            }
            else if (keyInfo.Key == ConsoleKey.Tab)
            {
                // Try to autocomplete
                string currentInput = input.ToString();
                string? completion = TryAutocomplete(currentInput);

                if (completion != null && completion != currentInput)
                {
                    // Replace with autocompleted text
                    input.Clear();
                    input.Append(completion);
                    cursorPosition = completion.Length;

                    // Redraw
                    RedrawLine(text: completion, cursorPosition, startColumn);
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                // Delete character before cursor
                if (cursorPosition > 0)
                {
                    input.Remove(cursorPosition - 1, 1);
                    cursorPosition--;

                    // Redraw the line
                    RedrawLine(input.ToString(), cursorPosition, startColumn);
                }
            }
            else if (keyInfo.Key == ConsoleKey.Delete)
            {
                // Delete character at cursor
                if (cursorPosition < input.Length)
                {
                    input.Remove(cursorPosition, 1);

                    // Redraw the line
                    RedrawLine(input.ToString(), cursorPosition, startColumn);
                }
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                // Move cursor left
                if (cursorPosition > 0)
                {
                    cursorPosition--;
                    Console.CursorLeft--;
                }
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                // Move cursor right
                if (cursorPosition < input.Length)
                {
                    cursorPosition++;
                    Console.CursorLeft++;
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                // Regular character input - insert at cursor position
                input.Insert(cursorPosition, keyInfo.KeyChar);
                cursorPosition++;

                // Redraw from cursor position to end
                RedrawLine(input.ToString(), cursorPosition, startColumn);
            }
        }
    }

    /// <summary>
    /// Attempts to autocomplete the current input with a builtin command
    /// </summary>
    private static string? TryAutocomplete(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // Get all builtin commands that start with the input
        var matches = CommandExecutor.GetBuiltinCommands()
            .Where(cmd => cmd.StartsWith(input, StringComparison.OrdinalIgnoreCase))
            .OrderBy(cmd => cmd)
            .ToList();

        // Return the first match if there's exactly one or multiple
        // If multiple, return the first one (user can press Tab again for next)
        return matches.FirstOrDefault();
    }

    /// <summary>
    /// Redraws the entire input line and positions cursor correctly
    /// </summary>
    private static void RedrawLine(string text, int cursorPosition, int startColumn)
    {
        // Move cursor to the start of the input text
        Console.CursorLeft = startColumn;
        
        // Write the entire text followed by spaces to clear any leftover characters
        Console.Write(text);
        Console.Write("   "); // Extra spaces to clear remaining characters
        
        // Move cursor back to the correct position
        Console.CursorLeft = startColumn + cursorPosition;
    }

    /// <summary>
    /// Clears the current line on the console
    /// </summary>
    private static void ClearCurrentLine(int length, int cursorPosition)
    {
        // Move cursor to beginning of input
        for (int i = 0; i < cursorPosition; i++)
        {
            Console.Write("\b");
        }

        // Overwrite entire line with spaces
        for (int i = 0; i < length; i++)
        {
            Console.Write(" ");
        }

        // Move cursor back to beginning
        for (int i = 0; i < length; i++)
        {
            Console.Write("\b");
        }
    }
}
