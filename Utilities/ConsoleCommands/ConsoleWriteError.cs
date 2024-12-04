using System;

namespace Utilities.ConsoleCommands
{
    public static class ConsoleWriteError
    {

        public static void WriteTheError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{Environment.NewLine}{error}");
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}