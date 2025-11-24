[CommandName("date")]
public class DateCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        string? format = args.Count > 0 ? args[0] : null;
        if (format == null)
        {
            Console.WriteLine(DateTime.Now.ToString("ddd MMM dd HH:mm:ss yyyy"));
        }
        else
        {
            if (format.StartsWith("+"))
            {
                string yyyy = DateTime.Now.ToString("yyyy");
                string MM = DateTime.Now.ToString("MM");
                string dd = DateTime.Now.ToString("dd");
                string HH = DateTime.Now.ToString("HH");
                string mm = DateTime.Now.ToString("mm");
                string ss = DateTime.Now.ToString("ss");
                string dddd = DateTime.Now.ToString("dddd");
                string MMMM = DateTime.Now.ToString("MMMM");

                // Use StringBuilder for efficient string manipulation
                var sb = new System.Text.StringBuilder(format.Substring(1));
                sb.Replace("%%", "\x00PERCENT\x00") // Temporary placeholder
                  .Replace("%Y", yyyy)
                  .Replace("%m", MM)
                  .Replace("%d", dd)
                  .Replace("%H", HH)
                  .Replace("%M", mm)
                  .Replace("%S", ss)
                  .Replace("%A", dddd)
                  .Replace("%B", MMMM)
                  .Replace("\x00PERCENT\x00", "%"); // Restore as single %

                Console.WriteLine(sb.ToString());
            }
            else
            {
                Console.WriteLine(DateTime.Now.ToString("ddd MMM dd HH:mm:ss yyyy"));
            }
        }
        return true;
    }
}
