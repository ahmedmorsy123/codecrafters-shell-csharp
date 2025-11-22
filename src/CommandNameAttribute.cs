[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CommandNameAttribute : Attribute
{
    public string Name { get; }

    public CommandNameAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Command name cannot be null or empty", nameof(name));
        }

        Name = name;
    }
}
