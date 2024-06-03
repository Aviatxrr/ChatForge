using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Spectre.Console;

public class Program
{

    public static void Main(string[] args)
    {
        
        Panel panel = new Panel("");

        AnsiConsole.Live(panel).Start(ctx =>

            {
                while (true)
                {
                    panel.Border = BoxBorder.Ascii;
                    panel.Expand = true;
                    panel.Height = Console.WindowHeight;
                    ctx.Refresh();
                }
            });
    }
}