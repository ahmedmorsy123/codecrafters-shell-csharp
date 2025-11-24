using System;

/// <summary>
/// Base class for shell-specific exceptions.
/// </summary>
public class ShellException : Exception
{
    public ShellException() { }
    public ShellException(string message) : base(message) { }
    public ShellException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a command name is not found.
/// </summary>
public class CommandNotFoundException : ShellException
{
    public CommandNotFoundException(string commandName)
        : base($"{commandName}: Command not found.") { }
}

/// <summary>
/// Thrown when an error occurs during command execution.
/// </summary>
public class ExecutionException : ShellException
{
    public ExecutionException(string commandName, Exception inner)
        : base($"Error executing command '{commandName}'.", inner) { }
}
