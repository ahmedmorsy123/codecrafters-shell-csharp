# Copilot Instructions for Shell Implementation

## Architecture Overview

This is a **POSIX-compliant shell** in C# with a plugin-based command system. The architecture separates concerns:

- **Command Discovery**: Reflection-based registration via `[CommandName("cmd")]` attributes
- **Execution Flow**: `main.cs` → `CommandParser.ParsePipeline()` → `CommandExecutor.ExecutePipeline()` → Individual commands
- **Console Abstraction**: `IConsole` interface allows dependency injection for testing (see `MockConsole` vs `RealConsole`)
- **Pipeline Architecture**: Builtin commands capture output to `StringWriter`, external commands use `Process` with stdio redirection

## Critical Patterns

### Adding New Builtin Commands

1. Create class in `src/Commands/` implementing `ICommand`
2. Add `[CommandName("yourcommand")]` attribute - autodiscovered by reflection in `CommandExecutor.RegisterCommands()`
3. Return `true` to continue shell, `false` to exit (like `ExitCommand`)
4. See `EchoCommand.cs` for minimal example

### Console I/O Testing

**Always use `IConsole` abstraction**, never direct `Console.*` calls in testable code:

```csharp
// InputReader and testable components
_console.Write("text");  // ✅ Testable via MockConsole

// Builtin commands can use Console directly
Console.WriteLine("output");  // ✅ OK - captured by OutputRedirector in pipelines
```

Test setup pattern (see any test file):

```csharp
public TestClass() {
    _output = new StringWriter();
    Console.SetOut(_output);
}
```

### Static State Management

**Critical**: Tests use static singletons (`Autocomplete`, `PipelineHistory`). Always clear in test constructors:

```csharp
public CommandExecutorTests() {
    Autocomplete.Clear();
    PipelineHistory.ClearHistory();
}
```

## Build & Test Workflow

```bash
# Build
dotnet build

# Run all 182 tests (serial execution required - see xunit.runner.json)
dotnet test

# Run specific test category
dotnet test --filter "FullyQualifiedName~Commands"    # Builtin command tests
dotnet test --filter "FullyQualifiedName~Core"        # Core functionality
dotnet test --filter "FullyQualifiedName~Integration" # End-to-end pipelines

# Run shell
dotnet run
```

## Project-Specific Conventions

### Exception Handling

- `CommandNotFoundException`: Thrown by `CommandExecutor.Execute()` for unknown commands - **caught in main loop, not silently handled**
- `ExecutionException`: Wrapper for command execution errors
- Use `ArgumentOutOfRangeException` for missing required arguments (not `IndexOutOfRangeException`)

### File Organization

- **One test file per command** in `tests/Commands/`
- **One test file per core component** in `tests/Core/`
- Mock infrastructure in `tests/Mocks/`
- All commands in `src/Commands/` directory

### Code Style

- Use discard pattern `_` for intentionally ignored values: `_ = _console.Read();` not `int next = ...`
- Document "why" in comments, not "what": `// consume \n if present` explains intent
- No-op methods should be removed, not left with misleading docs (see removed `MoveCursorRight`)

## Data Flow Patterns

### Pipeline Execution

1. **Parser**: `CommandParser.ParsePipeline("cmd1 | cmd2")` creates `Pipeline` with `List<Command>`
2. **Executor**: `ExecutePipeline()` determines builtin vs external for each command
3. **Builtin**: Redirect `Console.Out` to `StringWriter`, capture output, pass to next command
4. **External**: Start `Process`, connect previous stdout to current stdin via `CopyToAsync`
5. **Final output**: Last command writes to console (or redirected file)

### History Management

- **In-memory**: `PipelineHistory` static class with position tracking
- **Persistence**: Load from `$HISTFILE` on startup, save on exit (see `main.cs`)
- **Navigation**: `GetPrevious()`/`GetNext()` maintains current position, `ListHistory()` returns all

### Autocomplete System

- **Trie-based**: `Autocomplete` wraps `Trie` for O(k) prefix matching
- **Registration**: Builtin commands registered in `CommandExecutor` constructor, PATH executables scanned via `RegisterPathExecutables()`
- **Completion**: Single match → add space, multiple matches → double-tab shows list

## Testing Philosophy

- **182 comprehensive tests** with strict serial execution (`xunit.runner.json: maxParallelThreads: 1`)
- Test both **interactive** and **redirected input** modes (see `InputReaderTests`)
- Use `Path.GetTempFileName()` for file I/O tests, always cleanup in `finally` blocks
- Mock console for unit tests, real process execution for integration tests

## Common Gotchas

1. **Console.CursorLeft throws IOException** when output is redirected - wrap in try/catch or avoid in tests
2. **Pipeline exit codes** only return last command's exit code (known limitation)
3. **Static state** in `Autocomplete`/`PipelineHistory` must be cleared between tests
4. **Reflection discovery** means commands won't work without `[CommandName]` attribute
5. **Test directory** `testbin/` is added to PATH in `main.cs` for testing external commands
