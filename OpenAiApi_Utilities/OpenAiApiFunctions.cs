using Newtonsoft.Json;
using OpenAiApi_Utilities.Data;
using OpenAiApi_Utilities.Data.Dictionaries;
using OpenAiApi_Utilities.Data.Enums;
using OpenAiApi_Utilities.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenAiApi_Utilities
{
    public static class OpenAiApiFunctions
    {

        public static string Whisper_ReturnOnlyText(string audioUrl, GptModel gptModel = GptModel.whisper1)
        {
            var transcriptionAnswer = OpenAiApiBaseFunctions.CreateTranscription(audioFileUrl: audioUrl, gptModel: gptModel);
            Transcription transcriptionObj = JsonConvert.DeserializeObject<Transcription>(transcriptionAnswer);
            return transcriptionObj.Text;
        }

        /// <summary>
        /// Zapytanie do moderacji
        /// </summary>
        /// <param name="text"></param>
        /// <param name="modModel"></param>
        /// <returns>JSON OpenAiApi_Utilities.ResponseTypes.Moderations</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Moderations(string text, ModerationsModel modModel = ModerationsModel.latest)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            var json = JsonConvert.SerializeObject(new
            {
                input = text,
                model = OpenAiDictionaries.ModerationsModels.Where(x => x.Key == modModel).Single().Value,
            });

            return OpenAiApiBaseFunctions.PostRequest(json, ApiPaths.ModerationsApiPath);
        }

        public static string Completions_ReturnOnlyGptAnswer(object[] messagesTable, double modelTemp = 0.7, GptModel gptModel = GptModel.gpt35turbo)
        {
            var completionsAnswer = Completions(messagesTable: messagesTable, modelTemp: modelTemp, gptModel: gptModel);
            ChatCompletion chatCompletionObj = JsonConvert.DeserializeObject<ChatCompletion>(completionsAnswer);
            return chatCompletionObj.Choices[0].Message.Content;
        }

        /// <summary>
        /// Zapytanie do GPT
        /// </summary>
        /// <param name="messagesTable">tabela wszystkich wiadomości dla GPT z podziałem na role</param>
        /// <param name="modelTemp"></param>
        /// <param name="gptModel"></param>
        /// <returns>JSON OpenAiApi_Utilities.ResponseTypes.ChatCompletion</returns>
        public static string Completions(object[] messagesTable, double modelTemp = 0.7, GptModel gptModel = GptModel.gpt35turbo)
        {
            var json = JsonConvert.SerializeObject(new
            {
                model = OpenAiDictionaries.GptModels.Where(x => x.Key == gptModel).Single().Value,
                messages = messagesTable,
                temperature = modelTemp,
            });

            return OpenAiApiBaseFunctions.PostRequest(json, ApiPaths.ChatCompletionsApiPath);
        }

        public static List<double> Embeddings_ReturnOnlyEmbedding(string input, GptModel gptModel = GptModel.textEmbeddinAda002)
        {
            var embeddingsAnswer = Embeddings(input: input, gptModel: gptModel);
            Embeddings embeddingObj = JsonConvert.DeserializeObject<Embeddings>(embeddingsAnswer);
            return embeddingObj.Data[0].Embedding;
        }

        public static string Embeddings(string input, GptModel gptModel = GptModel.textEmbeddinAda002)
        {
            var json = JsonConvert.SerializeObject(new
            {
                input = input,
                model = OpenAiDictionaries.GptModels.Where(x => x.Key == gptModel).Single().Value,
            });

            return OpenAiApiBaseFunctions.PostRequest(json, ApiPaths.EmbeddingsApiPath);
        }

        public static string GuardRails_DoesAnswerAnswersTheQuestion(string question, string answer)
        {
            var systemMessage =
                $"You will be provided with a qestion and an answer.{Environment.NewLine}" +
                //$" in a following pattern:{Environment.NewLine}" +
                //$"###{Environment.NewLine}" +
                //$"question: ...{Environment.NewLine}" +
                //$"answer: ...{Environment.NewLine}" +
                //$"###{Environment.NewLine}" +
                $"Say only YES or NO if the answer is about the question.{Environment.NewLine}" +
                $"Say 'I don'know' if you're not sure.{Environment.NewLine}";

            var userMessage = $"{question} - {answer}";

            var messagesTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                systemMessage: systemMessage,
                userMessage: userMessage);

            return GuardRails(messagesTable: messagesTable);
        }

        public static string GuardRails(object[] messagesTable, double modelTemp = 0.1, GptModel gptModel = GptModel.gpt40613)
        {
            var json = JsonConvert.SerializeObject(new
            {
                model = OpenAiDictionaries.GptModels.Where(x => x.Key == gptModel).Single().Value,
                messages = messagesTable,
                temperature = modelTemp,
            });

            return OpenAiApiBaseFunctions.PostRequest(json, ApiPaths.ChatCompletionsApiPath);
        }

    }
}