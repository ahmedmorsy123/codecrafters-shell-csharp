class Program
{
    static void Main(string[] args)
    {
        try
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

                Pipeline pipeline = CommandParser.ParsePipeline(commandLine);

                // Execute the pipeline and check if we should continue
                bool shouldContinue = executor.ExecutePipeline(pipeline);
                if (!shouldContinue)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
