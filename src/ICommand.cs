public interface ICommand
{
    /// <summary>
    /// Executes the command with the given arguments
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>True if the shell should continue running, false if it should exit</returns>
    bool Execute(IReadOnlyList<string> args);
}
