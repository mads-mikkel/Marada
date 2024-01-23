using static System.Console;

namespace Marada.Cli
{
    public class CliEntry
    {
        public static void Main(string[] args)
        {
            CliProgram p = new();
            p.Init();
        }


    }

    internal class CliProgram
    {
        private string version = "0.9";

        public void Init()
        {
            DisplayMessage($"MARADA Cli Tool v. {version}");
        }

        public void DisplayMessage(string message, bool lineBreakAfter = true, bool lineBreakBefore = false)
        {
            if (lineBreakAfter)
            {
                WriteLine(message);
            }
            else 
            {
                Write(message);
            }
        }
    }
}