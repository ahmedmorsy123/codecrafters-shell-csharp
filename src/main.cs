using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // For testing: add testbin directory to PATH
        string testBinPath = Path.Combine(Directory.GetCurrentDirectory(), "testbin");
        string? currentPath = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(currentPath))
        {
            Environment.SetEnvironmentVariable("PATH", testBinPath + Path.PathSeparator + currentPath);
        }
        else
        {
            Environment.SetEnvironmentVariable("PATH", testBinPath);
        }

        PipelineHistory.LoadHistoryFromFile();

        var executor = new CommandExecutor();
        while (true)
        {
            Console.Write("$ ");

            string? commandLine = InputReader.ReadLine();
            if (commandLine == null)
            {
                // EOF on redirected input — stop the shell loop
                break;
            }

            try
            {
                Pipeline pipeline = CommandParser.ParsePipeline(commandLine);

                // Execute the pipeline and check if we should continue
                bool shouldContinue = executor.ExecutePipeline(pipeline);
                if (!shouldContinue)
                {
                    break;
                }
            }
            catch (CommandNotFoundException ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            catch (ExecutionException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }

        // Save history before exiting
        PipelineHistory.SaveHistoryToFile();
    }
}
