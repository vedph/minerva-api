using Minerva.Services;
using System;

namespace Minerva.Cli;

internal static class Program
{
    static void Main(string[] args)
    {
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" MINERVA TEST ");
        Console.ResetColor();
        ItaTextQueryAssistant assistant = new();

        while (true)
        {
            Console.Write("Text (?=help): ");
            string? text = Console.ReadLine();
            if (string.IsNullOrEmpty(text)) break;

            if (text == "help" || text == "?")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(assistant.GetHelp());
                Console.ResetColor();
                continue;
            }

            string? query = assistant.ToQuery(text);

            if (query != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(query);
                Console.ResetColor();
                Console.WriteLine();
            }
        }
    }
}
