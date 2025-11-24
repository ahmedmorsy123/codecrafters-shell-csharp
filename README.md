# Custom Shell - C# Implementation

A POSIX-compliant shell built in C# with support for builtin commands, external programs, pipelines, command history, and more.

## Features

### Core Functionality

- **Interactive REPL**: Command prompt with real-time input handling
- **Builtin Commands**: 18 commands implemented
  - **File Operations**: `cat`, `pwd`, `cd`, `type`, `which`
  - **Output**: `echo`, `date`, `env`, `whoami`, `clear`/`cls`
  - **Control Flow**: `true`, `false`, `sleep`, `exit`
  - **History**: `history` with persistent storage
  - **Aliases**: `alias`, `unalias` for command shortcuts
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
├── ShellException.cs            # Custom exceptions
├── IConsole.cs                  # Console abstraction interface
├── RealConsole.cs               # Real console implementation
├── ConsoleHelper.cs             # Console utility methods
├── AliasManager.cs              # Command alias management
└── Commands/
    ├── CdCommand.cs             # Change directory
    ├── EchoCommand.cs           # Print arguments
    ├── ExitCommand.cs           # Exit shell
    ├── PwdCommand.cs            # Print working directory
    ├── TypeCommand.cs           # Show command type/location
    ├── HistoryCommand.cs        # Display command history
    ├── ClearCommand.cs          # Clear console screen
    ├── CatCommand.cs            # Concatenate and display files
    ├── EnvCommand.cs            # Display environment variables
    ├── DateCommand.cs           # Display date/time with formatting
    ├── WhoamiCommand.cs         # Display current username
    ├── WhichCommand.cs          # Locate commands in PATH
    ├── TrueCommand.cs           # Always succeed (exit code 0)
    ├── FalseCommand.cs          # Always fail (exit code 1)
    ├── SleepCommand.cs          # Delay execution
    ├── AliasCommand.cs          # Create command aliases
    └── UnaliasCommand.cs        # Remove command aliases

tests/
├── Commands/
│   ├── CdCommandTests.cs        # cd command tests (11 tests: tilde, parent dir, relative paths, errors)
│   ├── ClearCommandTests.cs     # clear/cls command tests (1 test)
│   ├── EchoCommandTests.cs      # echo command tests (3 tests)
│   ├── ExitCommandTests.cs      # exit command tests (2 tests)
│   ├── HistoryCommandTests.cs   # history command tests (14 tests: list, read, write, append, edge cases)
│   ├── PwdCommandTests.cs       # pwd command tests (1 test)
│   ├── TypeCommandTests.cs      # type command tests (5 tests: builtin/external detection)
│   ├── CatCommandTests.cs       # cat command tests (14 tests: files, stdin, errors, concatenation)
│   ├── EnvCommandTests.cs       # env command tests (9 tests: all vars, specific vars, case-insensitive)
│   ├── DateCommandTests.cs      # date command tests (15 tests: default, format specifiers, escaping)
│   ├── WhoamiCommandTests.cs    # whoami command tests (5 tests: username display, cross-platform)
│   ├── WhichCommandTests.cs     # which command tests (10 tests: builtins, PATH search, Windows extensions)
│   ├── TrueCommandTests.cs      # true command tests (5 tests: always succeeds)
│   ├── FalseCommandTests.cs     # false command tests (5 tests: always fails)
│   ├── SleepCommandTests.cs     # sleep command tests (10 tests: timing, validation, limits)
│   ├── AliasCommandTests.cs     # alias command tests (14 tests: create, list, display)
│   └── UnaliasCommandTests.cs   # unalias command tests (12 tests: remove, -a flag, errors)
├── Core/
│   ├── AutocompleteTests.cs     # Tab completion tests (15 tests)
│   ├── CommandExecutorTests.cs  # Command execution tests (19 tests)
│   ├── CommandParserTests.cs    # Parser and pipeline tests (16 tests)
│   ├── ConsoleHelperTests.cs    # Console helper tests (14 tests: cursor, redraw, clear)
│   ├── HistoryPersistenceTests.cs # History file I/O tests (7 tests)
│   ├── InputReaderTests.cs      # Input handling tests (30 tests: interactive & redirected input, arrows, tab, history)
│   ├── ModelsTests.cs           # Data model and exception tests (12 tests)
│   ├── OutputRedirectorTests.cs # Output redirection tests (8 tests)
│   ├── PipelineHistoryTests.cs  # History navigation tests (14 tests)
│   ├── RealConsoleTests.cs      # Real console wrapper tests (8 tests)
│   └── TrieTests.cs             # Trie data structure tests (13 tests)
├── Integration/
│   └── IntegrationTests.cs      # End-to-end pipeline tests (20 tests)
├── Mocks/
│   └── MockConsole.cs           # Console mock for testing (redirected & interactive modes)
├── xunit.runner.json            # xUnit configuration (serial execution)
└── CodecraftersShell.Tests.csproj
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

### Test

```bash
# Run all tests
dotnet test tests/CodecraftersShell.Tests.csproj

# Run specific test category
dotnet test --filter "FullyQualifiedName~Commands"     # Command tests
dotnet test --filter "FullyQualifiedName~Core"         # Core functionality tests
dotnet test --filter "FullyQualifiedName~Integration"  # Integration tests

# Run with detailed output
dotnet test --verbosity normal
```

The test suite includes:

- **281 comprehensive unit tests** covering all components
- **Commands tests** (135 tests): Individual test files for each of the 18 builtin commands
  - **Original commands** (37 tests):
    - CdCommand (11 tests): Tilde expansion, parent directory (..), relative/absolute paths, error handling
    - HistoryCommand (14 tests): List, limit, read/write/append to file, empty history, edge cases
    - EchoCommand (3 tests), PwdCommand (1 test), ExitCommand (2 tests), TypeCommand (5 tests), ClearCommand (1 test)
  - **New commands** (99 tests):
    - CatCommand (14 tests): File reading, stdin support for pipelines, multiple files, error handling
    - DateCommand (15 tests): Default format, custom format specifiers (%Y, %m, %d, etc.), %% escaping
    - AliasCommand (14 tests): Create/update aliases, list all, display specific, multiple definitions
    - UnaliasCommand (12 tests): Remove aliases, -a flag for all, error messages, validation
    - EnvCommand (9 tests): Display all vars, specific vars, case-insensitive matching, empty values
    - WhichCommand (10 tests): Builtin detection, PATH search, Windows .exe/.bat extensions
    - SleepCommand (10 tests): Valid delays, zero sleep, error handling, 1-hour maximum limit
    - WhoamiCommand (5 tests): Username display, cross-platform (USERNAME/USER), error fallback
    - TrueCommand (5 tests): Always returns success, ignores arguments
    - FalseCommand (5 tests): Sets exit code 1, continues shell execution
- **Core tests** (144 tests): Parser, executor, autocomplete, history, I/O, console utilities
  - InputReader (27 tests): Interactive mode, redirected mode, arrows, tab completion, history navigation
  - CommandParser (26 tests): Pipelines, quotes, arguments, edge cases, redirection
  - CommandExecutor (19 tests): Builtin commands, external commands, PATH resolution, pipelines
  - ConsoleHelper (12 tests): Cursor movement, line clearing, text redrawing
  - ModelsTests (12 tests): Data structures, exceptions, command/pipeline models
  - HistoryPersistenceTests (11 tests): File I/O, save/load, tilde expansion
  - PipelineHistoryTests (8 tests): Navigation, add/clear operations
  - RealConsole (8 tests): Console wrapper implementation
  - OutputRedirectorTests (6 tests): Stdout/stderr redirection
  - AutocompleteTests (6 tests): Trie-based prefix matching
  - TrieTests (9 tests): Trie data structure operations
- **Integration tests** (2 tests): End-to-end pipeline and redirection scenarios
- **Mock infrastructure**: Console abstraction with support for both interactive and redirected input modes

### Test Coverage

Key coverage metrics:

- **InputReader**: 56% → Comprehensive tests for both interactive and redirected input paths
- **CommandExecutor**: 47% → Extended tests for PATH resolution, redirection, pipeline execution
- **ConsoleHelper**: Tests for all utility methods (cursor, clear, redraw)
- **RealConsole**: Complete test coverage for console wrapper
- All builtin commands fully tested with edge cases and error conditions

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

### New Commands

```bash
# Environment variables
$ env
PATH=/usr/bin:/bin
HOME=/home/user
...

$ env PATH
PATH=/usr/bin:/bin

# Date and time
$ date
Mon Nov 24 15:30:45 2025

$ date +%Y-%m-%d
2025-11-24

# File operations
$ cat file.txt
Contents of file.txt

$ echo "line 1" | cat
line 1

# Command location
$ which echo
echo is a shell builtin

$ which python
/usr/bin/python

# Aliases
$ alias ll='ls -la'
$ alias
alias ll='ls -la'

$ unalias ll
$ unalias -a  # Remove all aliases

# User info
$ whoami
john_doe

# Utility commands
$ true && echo "success"
success

$ false || echo "failed"
failed

$ sleep 2  # Pauses for 2 seconds
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

See [CONTRIBUTING.md](CONTRIBUTING.md) for a detailed guide on adding new commands and understanding the codebase.

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
- Aliases are not expanded in command execution (stored but not yet used)

## Future Enhancements

- Alias expansion in command execution
- Shell scripting support (.sh file execution)
- Job control (fg, bg, jobs)
- More sophisticated globbing
- Brace expansion
- Command substitution
- Environment variable expansion in commands

## License

This project was built as part of the CodeCrafters "Build Your Own Shell" challenge.

## Acknowledgments

Built following the [CodeCrafters Shell Challenge](https://app.codecrafters.io/courses/shell/overview)
