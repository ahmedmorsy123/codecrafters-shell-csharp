# Custom Shell - C# Implementation

A POSIX-compliant shell built in C# with support for builtin commands, external programs, pipelines, command history, and more.

## Features

### Core Functionality

- **Interactive REPL**: Command prompt with real-time input handling
- **Builtin Commands**: `cd`, `pwd`, `echo`, `type`, `exit`, `history`
- **External Commands**: Execute any program available in PATH
- **Command Parsing**: Handles quoted arguments and special characters

### Advanced Features

- **Pipelines**: Chain commands with `|` operator (supports multiple commands)
  - Works with builtin-to-external, external-to-external, and mixed pipelines
  - Example: `echo hello | wc` or `cat file.txt | grep pattern | head -n 10`
- **Tab Completion**: Autocomplete for builtin commands and PATH executables
  - Press Tab once for single match completion
  - Press Tab twice to show all matches
  - Supports partial matching and longest common prefix
- **Command History**:
  - Navigate with Up/Down arrow keys
  - `history` command to list all commands
  - `history N` to show last N commands
  - Persistent history saved to file on exit (set `HISTFILE` environment variable)
- **Output Redirection**: Support for redirecting command output (inherited from pipeline implementation)

### Input Handling

- **Interactive Mode**: Full keyboard support (arrows, backspace, delete, insert)
- **Redirected Input**: Handles piped/file input for automated testing
- **Cross-platform**: Works on Windows, Linux, and macOS

## Architecture

### Project Structure

```
src/
├── main.cs                      # Entry point and main shell loop
├── Command.cs                   # Command data structure
├── CommandExecutor.cs           # Command execution engine
├── CommandNameAttribute.cs      # Attribute for command registration
├── commandParser.cs             # Command line parser and Pipeline class
├── ICommand.cs                  # Command interface
├── InputReader.cs               # Interactive and redirected input handling
├── Autocomplete.cs              # Tab completion logic
├── Trie.cs                      # Trie data structure for autocomplete
├── PipelineHistory.cs           # Command history management
├── OutputRedirector.cs          # Output redirection support
├── RedirectionInfo.cs           # Redirection metadata
└── Commands/
    ├── CdCommand.cs             # Change directory
    ├── EchoCommand.cs           # Print arguments
    ├── ExitCommand.cs           # Exit shell
    ├── PwdCommand.cs            # Print working directory
    ├── TypeCommand.cs           # Show command type/location
    └── HistoryCommand.cs        # Display command history
```

### Key Components

#### Command Executor

- Discovers and registers builtin commands via reflection
- Executes builtin and external commands
- Handles pipeline execution with proper stdin/stdout/stderr redirection
- Manages process lifecycle for external commands

#### Pipeline Support

- Parses command lines with `|` separator
- Connects process stdout to stdin for chaining
- Handles both builtin and external commands in pipelines
- Supports builtin output piping to external commands

#### Autocomplete System

- Trie-based prefix matching for fast lookups
- Scans PATH directories for external executables
- Returns longest common prefix for partial matches
- Marks complete vs incomplete matches for proper spacing

#### History Management

- In-memory history with position tracking
- Arrow key navigation (up/down)
- Persistent storage to file (HISTFILE environment variable)
- Automatic save on graceful exit

## Building and Running

### Prerequisites

- .NET 9.0 SDK

### Build

```bash
dotnet build --configuration Release --output ./bin/dist codecrafters-shell.csproj
```

### Run

```bash
dotnet run
```

### Set History File (Optional)

```bash
export HISTFILE=~/.shell_history
dotnet run
```

## Usage Examples

### Basic Commands

```bash
$ pwd
/home/user/projects

$ echo "Hello, World!"
Hello, World!

$ type echo
echo is a shell builtin

$ type cat
cat is /usr/bin/cat
```

### Pipelines

```bash
$ echo "apple\nbanana\ncherry" | grep a
apple
banana

$ cat large_file.txt | grep error | wc -l
42

$ tail -f log.txt | head -n 5
```

### History

```bash
$ history
    1  echo hello
    2  pwd
    3  history

$ history 2
    2  pwd
    3  history

# Navigate with Up/Down arrows to recall previous commands
```

### Tab Completion

```bash
$ ec<TAB>
$ echo          # Completes to "echo "

$ my<TAB><TAB>
myapp  mytest  mytool    # Shows all matches starting with "my"
```

## Development Notes

### Adding New Builtin Commands

1. Create a new class in `src/Commands/` implementing `ICommand`
2. Add `[CommandName("yourcommand")]` attribute
3. Implement `Execute(IReadOnlyList<string> args)` method
4. Return `true` to continue shell, `false` to exit

Example:

```csharp
[CommandName("hello")]
public class HelloCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        Console.WriteLine("Hello, Shell!");
        return true;
    }
}
```

### Pipeline Execution Flow

1. Parse command line into Pipeline object
2. For each command in pipeline:
   - Check if builtin (capture output to string)
   - Or start external process
   - Connect stdout→stdin between processes
3. Wait for output from last command
4. Wait for all pipe tasks to complete
5. Wait for all processes to exit

### History Persistence

- History loaded from `$HISTFILE` on startup
- Each command added to in-memory list
- All history saved to `$HISTFILE` on exit
- Supports `~` expansion and absolute/relative paths

## Known Limitations

- Exit codes from pipelines return the last command's exit code only
- No support for `&&`, `||`, `;` operators
- No background processes (`&`)
- No input redirection (`<`)
- No append redirection (`>>`)
- No globbing (`*`, `?`)

## Future Enhancements

- Command aliases
- Shell scripting support
- Job control (fg, bg, jobs)
- More sophisticated globbing
- Brace expansion
- Command substitution

## License

This project was built as part of the CodeCrafters "Build Your Own Shell" challenge.

## Acknowledgments

Built following the [CodeCrafters Shell Challenge](https://app.codecrafters.io/courses/shell/overview)
