class Program
{
    static void Main(string[] args)
    {
        // TODO: Uncomment the code below to pass the first stage
        while (true)
        {
             Console.Write("$ ");
        
            string? command = Console.ReadLine(); 
            Console.WriteLine($"{command}: command not found");
        }

    }           
}
