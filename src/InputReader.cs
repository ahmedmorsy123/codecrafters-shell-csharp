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
        // Check if stdin is redirected (e.g., from a pipe or file)
        // If redirected, read and echo character by character for test visibility
        if (Console.IsInputRedirected)
        {
            StringBuilder line = new StringBuilder();
            int ch;
            while ((ch = Console.Read()) != -1 && ch != '\n' && ch != '\r')
            {
                char c = (char)ch;
                line.Append(c);
                Console.Write(c); // Echo immediately so tests can see it
            }
            
            // Handle \r\n (Windows) vs \n (Unix)
            if (ch == '\r')
            {
                int next = Console.Read();
                if (next != '\n' && next != -1)
                {
                    // Put it back... but we can't, so just ignore
                }
            }
            
            return line.ToString();
        }

        // Interactive mode with autocomplete
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
                // Autocomplete using Trie
                string currentInput = input.ToString();
                string? completion = CommandExecutor.GetAutocomplete(currentInput);

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
}
