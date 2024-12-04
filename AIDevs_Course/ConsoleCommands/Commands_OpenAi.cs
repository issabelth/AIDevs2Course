using Newtonsoft.Json;
using OpenAiApi_Utilities;
using OpenAiApi_Utilities.ResponseTypes;
using System;
using System.Threading.Tasks;

namespace AIDevs_Course.ConsoleCommands
{

    /// <summary>
    /// Commands_OpenAi - akcje związane z OpenAI
    /// </summary>
    public partial class CommandsList
    {

        private static async Task<int> FirstGptContact()
        {
            Console.WriteLine("Podaj wiadomość systemową:");
            var systemMessage = Console.ReadLine();
            Console.WriteLine("Podaj wiadomość usera:");
            var userMessage = Console.ReadLine();

            var messagesTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                systemMessage: systemMessage,
                userMessage: userMessage);
            
            var gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: messagesTable);

            Console.WriteLine(gptAnswer);
            return await Task.FromResult(0);
        }

        private static async Task<int> GuardRails_DoesAnswerAnswersTheQuestion()
        {
            Console.WriteLine("Napisz pytanie:");
            var question = Console.ReadLine();
            Console.WriteLine("Napisz odpowiedź:");
            var answer = Console.ReadLine();

            var result = OpenAiApiFunctions.GuardRails_DoesAnswerAnswersTheQuestion(question: question, answer: answer);

            ChatCompletion chatCompletionObj = JsonConvert.DeserializeObject<ChatCompletion>(result);
            string gptAnswer = chatCompletionObj.Choices[0].Message.Content;

            Console.WriteLine($"Czy odpowiedź dotyczy pytania: {gptAnswer}");
            return await Task.FromResult(0);
        }

    }
}