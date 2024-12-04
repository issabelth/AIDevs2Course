using OpenAiApi_Utilities.Data;
using OpenAiApi_Utilities.Data.Dictionaries;
using OpenAiApi_Utilities.Data.Enums;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OpenAiApi_Utilities
{
    public static class OpenAiApiBaseFunctions
    {
        private static string GetOpenAiApiSecretKey()
        {
            string filePath = $@"D:\Iss\Praca\Kursy\AI Devs\OpenAI_RklApiKey.txt";

            try
            {
                return File.ReadAllText(filePath);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"The specified file was not found ({filePath})");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"An error occurred while reading file ({filePath}): {ex.Message}");
            }
            return null;
        }

        public static GptConversationEntry[] CreateBasicMessagesForChat(string systemMessage, string userMessage)
        {
            return new[]
                {
                    new GptConversationEntry
                    {
                        role = "system",
                        content = systemMessage,
                    },
                    new GptConversationEntry
                    {
                        role = "user",
                        content = userMessage,
                    },
                };
        }

        public static void AddNewEntryToTheConversation(ref GptConversationEntry[] existingConversation, string role, string msg)
        {
            var newEntry = new GptConversationEntry
            {
                role = role,
                content = msg,
            };
            existingConversation = existingConversation.Concat(new[] { newEntry }).ToArray();
        }

        public static string CreateTranscription(string audioFileUrl, GptModel gptModel = GptModel.whisper1)
        {
            using (var client = CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetOpenAiApiSecretKey());

                var audioFileContent = Task.Run(async () => await client.GetAsync(audioFileUrl)).Result;
                audioFileContent.EnsureSuccessStatusCode();

                var formDataContent = new MultipartFormDataContent
                {
                    { new ByteArrayContent(audioFileContent.Content.ReadAsByteArrayAsync().Result), "file", Path.GetFileName(audioFileUrl) },
                    { new StringContent(OpenAiDictionaries.GptModels.Where(x => x.Key == gptModel).Single().Value), "model" },
                };

                var response = Task.Run(async () => await client.PostAsync($"https://api.openai.com/v1/audio/transcriptions", formDataContent)).Result;
                response.EnsureSuccessStatusCode();
                var responseContent = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                return responseContent;
            }
        }

        public static string PostRequest(string json, string apiPath)
        {
            var client = CreateClient();
            var request = GetReadyToRequest(json, apiPath);

            client.DefaultRequestHeaders
               .Accept
               .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.SendAsync(request).Result;

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                Console.WriteLine($"Error {response.StatusCode}: {response.ReasonPhrase}");
                throw new Exception($"Error {response.StatusCode}: {response.ReasonPhrase}");
            }
        }

        public static  HttpClient CreateClient()
        {
            return new HttpClient()
            {
                BaseAddress = new Uri(@"https://api.openai.com/v1/")
            };
        }

        public static HttpRequestMessage GetReadyToRequest(string json, string apiPath)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, apiPath);
            request.Headers.Add("Authorization", "Bearer " + GetOpenAiApiSecretKey());

            request.Content = new StringContent(json, Encoding.UTF8, "application/json"); // Content-Type

            return request;
        }

    }
}