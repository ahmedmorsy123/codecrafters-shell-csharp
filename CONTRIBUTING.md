# Contributing to Codecrafters Shell

We welcome contributions! This guide will help you understand how to add new features and maintain the codebase.

## Adding a New Built-in Command

1.  **Create a new class** in `src/Commands` that implements the `ICommand` interface.
2.  **Add the `[CommandName("your_command")]` attribute** to your class. This automatically registers it with the `CommandExecutor`.
3.  **Implement the `Execute` method**. This method receives the arguments passed to the command.

Example:
```csharp
[CommandName("hello")]
public class HelloCommand : ICommand
{
    public bool Execute(string[] args)
    {
        Console.WriteLine("Hello, World!");
        return true; // Return true to keep the shell running
    }
}
```

## Autocomplete System

The autocomplete system is handled by the `Autocomplete` class.
- **Registration**: Commands are automatically registered for autocomplete when the shell starts.
- **Custom Logic**: You can add custom autocomplete logic by modifying `Autocomplete.GetSuggestion`.

## Testing

We use `xUnit` for testing.
- **Unit/Integration Tests**: Located in the `tests` directory.
- **Running Tests**: Run `dotnet test` in the root directory.
- **Mocking Console**: Use `MockConsole` to test interactive features without a real terminal.

## Code Style

- Follow standard C# coding conventions.
- Ensure all new code is covered by tests where possible.
