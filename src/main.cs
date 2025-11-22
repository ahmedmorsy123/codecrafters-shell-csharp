class Program
{
    static void Main(string[] args)
    {
        try
        {
            
            var executor = new CommandExecutor();
            while (true)
            {
                Console.Write("$ ");
            
                string commandLine = Console.ReadLine()!; 
                Command command = CommandParser.Parse(commandLine);
                
                // Execute the command and check if we should continue
                bool shouldContinue = executor.Execute(command);
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
