using AIDevs_Course.ConsoleCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.ConsoleCommands;
using Utilities.ExtensionsMethods;

namespace AIDevs_Course
{
    class Program
    {

        private static readonly List<CommandAction> _cmdOpenAiList = CommandsList.OpenAiCommandsList;
        private static readonly List<CommandAction> _cmdAiDevsList = CommandsList.AiDevsCommandsList;
        private static readonly List<CommandAction> _cmdQdrantList = CommandsList.QdrantCommandsList;

        static async Task Main(string[] args)
        {
            Console.WriteLine(
                $"W który dział chcesz wejść?" +
                $"{Environment.NewLine}Open AI" +
                $"{Environment.NewLine}AI Devs" +
                $"{Environment.NewLine}Qdrant");

            var myStartAct = Console.ReadLine().ClearWhiteSpacesAndToLower();

            switch (myStartAct)
            {
                case "openai":
                    {
                        await ChooseMethodAsync(_cmdOpenAiList);
                        break;
                    }
                case "aidevs":
                    {
                        await ChooseMethodAsync(_cmdAiDevsList);
                        break;
                    }
                case "qdrant":
                    {
                        await ChooseMethodAsync(_cmdQdrantList);
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Nieprawidłowy dział.");
                        break;
                    }
            }

            Console.WriteLine("Naciśnij dowolny przycisk, aby wyjść.");
            Console.ReadKey();
        }

        private static async Task ChooseMethodAsync(List<CommandAction> cmdList)
        {
            do
            {
                var actionKey = GetChosenCommand(cmdList);
                var myAct = cmdList.Where(x => x.Key.ToLower() == actionKey).FirstOrDefault()?.Action;

                while (myAct == null)
                {
                    ConsoleWriteError.WriteTheError("Nie rozpoznano akcji.");
                    actionKey = GetChosenCommand(cmdList);
                    myAct = cmdList.Where(x => x.Key.ToLower() == actionKey).FirstOrDefault()?.Action;
                }

                Console.WriteLine();
                await myAct.Invoke();
            }
            while (UserOptionsMethods.YesOrNo(question: "Czy chcesz coś jeszcze zrobić?"));

            Console.WriteLine("Naciśnij dowolny przycisk, aby wyjść.");
            Console.ReadKey();
        }

        private static string GetChosenCommand(List<CommandAction> cmdList)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{Environment.NewLine}Możliwe opcje:");

            foreach (var command in cmdList)
            {
                Console.WriteLine($"{command.Key} - {command.ActionName}");
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            return Console.ReadLine().ClearWhiteSpacesAndToLower();
        }

    }
}