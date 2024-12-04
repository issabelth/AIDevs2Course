using AiDevsApi_Utilities.ResponseTypes;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace AiDevsApi_Utilities
{
    public abstract class AiDevsApiBaseFunctions
    {

        public static HttpClient CreateClient()
        {
            return new HttpClient()
            {
                BaseAddress = new Uri(uriString: @"https://tasks.aidevs.pl/"),
            };
        }

        public static string Authorize(HttpClient client, string taskName)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    method: HttpMethod.Post,
                    requestUri: $"token/{taskName}");

                request.Headers.Add("Accept", "application/json");

                var json = JsonConvert.SerializeObject(new
                {
                    apikey = "7cfec80f-5bfc-405d-b87c-dae1ecf4aad1",
                });

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = client.SendAsync(request: request).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(result);

                if (tokenResponse.Code < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Błąd przy autoryzacji. Porada: {tokenResponse.Message}{Environment.NewLine}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"Autoryzacja ok{Environment.NewLine}");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                return tokenResponse.Token;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static dynamic GetTask(HttpClient client, string token)
        {
            var result = GetTaskBase(client, token);

            string[] jsonLines = result.Split('\n');
            Console.ForegroundColor = ConsoleColor.Blue;

            foreach (string line in jsonLines)
            {
                Console.WriteLine($"{Environment.NewLine}{line}");
            }

            Console.ForegroundColor = ConsoleColor.White;

            var taskResponse = JsonConvert.DeserializeObject<dynamic>(result);

            if (taskResponse.Code < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd przy pobieraniu zadania. Porada: {taskResponse.Message}{Environment.NewLine}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Pobranie zadania ok{Environment.NewLine}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return taskResponse;
        }

        private static string GetTaskBase(HttpClient client, string token)
        {
            HttpRequestMessage request = new HttpRequestMessage(
                method: HttpMethod.Get,
                requestUri: $"task/{token}");

            request.Headers.Add("Accept", "application/json");

            var response = client.SendAsync(request: request).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return result;
        }

        public static dynamic PostTask(HttpClient client, string token, string question)
        {
            HttpRequestMessage request = new HttpRequestMessage(
                method: HttpMethod.Post,
                requestUri: $"task/{token}");

            request.Headers.Add("Accept", "application/json");

            // multipart/form-data , pole formularza
            var multipartFormDataContent = new MultipartFormDataContent
            {
                { new StringContent(question), "question" }
            };
            request.Content = multipartFormDataContent;

            var response = client.SendAsync(request: request).Result;
            var result = response.Content.ReadAsStringAsync().Result;

            var taskResponse = JsonConvert.DeserializeObject<dynamic>(result);

            if (taskResponse.code < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd przy pobieraniu zadania. Porada: {taskResponse.Message}{Environment.NewLine}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Pobranie zadania ok{Environment.NewLine}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return taskResponse;
        }

        public static AnswerResponse Answer(HttpClient client, string token, object answer)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    method: HttpMethod.Post,
                    requestUri: $"answer/{token}");

                request.Headers.Add("Accept", "application/json");

                var json = JsonConvert.SerializeObject(new
                {
                    answer = answer,
                });

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = client.SendAsync(request: request).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                var answerResponse = JsonConvert.DeserializeObject<AnswerResponse>(result);

                if (answerResponse.Code < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Błąd przy odpowiedzi. Porada: {answerResponse.Message}{Environment.NewLine}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"Odpowiedź ok{Environment.NewLine}");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                return answerResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

    }
}