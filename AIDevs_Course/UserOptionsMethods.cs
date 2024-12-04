using System;
using Utilities.ConsoleCommands;

namespace AIDevs_Course
{
    public static class UserOptionsMethods
    {

        /// <summary>
        /// Checking the user yes/no answer
        /// </summary>
        /// <returns>T -> return true, N -> return false</returns>
        public static bool YesOrNo(string question)
        {
            Console.WriteLine($"{Environment.NewLine}{question} (T/N)");
            var keyPressed = Console.ReadKey();

            while (keyPressed.Key != ConsoleKey.T &&
                keyPressed.Key != ConsoleKey.N)
            {
                ConsoleWriteError.WriteTheError("Błędny klawisz!");
                Console.WriteLine("Wciśnij T (tak) lub N (nie):");
                keyPressed = Console.ReadKey();
            }

            Console.WriteLine();

            return keyPressed.Key == ConsoleKey.T ? true : false;
        }

    }
}