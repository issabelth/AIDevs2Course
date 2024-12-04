using AiDevsApi_Utilities;
using AiDevsApi_Utilities.ResponseTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAiApi_Utilities;
using OpenAiApi_Utilities.Data.Enums;
using OpenAiApi_Utilities.ResponseTypes;
using QdrantApi_Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs_Course.ConsoleCommands
{

    /// <summary>
    /// Commands_AiDevs - akcje związane z AI Devs
    /// </summary>
    public partial class CommandsList
    {
        #region C01

        /*
         * Używając swojego klucza API, rozwiąż zadanie o nazwie HelloAPI.
         * Zadanie polega na pobraniu zmiennej cookie z powyższego taska i odesłaniu jej do serwera jako odpowiedzi w polu answer (w JSON).
         */
        private static async Task<int> HelloApi()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(HelloApi));

                var answer = AiDevsApiBaseFunctions.GetTask(client: client, token: token);

                AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: answer.cookie);
            }
            return await Task.FromResult(0);
        }

        /*
         * Zastosuj wiedzę na temat działania modułu do moderacji treści i rozwiąż zadanie o nazwie “moderation” z użyciem naszego API do sprawdzania rozwiązań.
         * Zadanie polega na odebraniu tablicy zdań (4 sztuki), a następnie zwróceniu tablicy z informacją, które zdania nie przeszły moderacji.
         * Jeśli moderacji nie przeszło pierwsze i ostatnie zdanie, to odpowiedź powinna brzmieć [1,0,0,1].
         * Pamiętaj, aby w polu ‘answer’ zwrócić tablicę w JSON, a nie czystego stringa.
         * P.S. wykorzystaj najnowszą wersję modelu do moderacji (text-moderation-latest)
         */
        private static async Task<int> Moderation()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Moderation));

                var taskInput = AiDevsApiBaseFunctions.GetTask(client: client, token: token)?.input;

                int[] answer = new int[taskInput.Count()];
                int i = 0;

                foreach (var input in taskInput)
                {
                    var moderationAnswer = OpenAiApiFunctions.Moderations(text: input);

                    Moderations moderationsObj = JsonConvert.DeserializeObject<Moderations>(moderationAnswer);
                    var flagged = moderationsObj.Results[0].Flagged;
                    Console.WriteLine($"{input}");
                    answer[i++] = flagged ? 1 : 0;
                }

                AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: answer);
            }
            return await Task.FromResult(0);
        }

        /*
         * Napisz wpis na bloga (w języku polskim) na temat przyrządzania pizzy Margherity.
         * Zadanie w API nazywa się ”blogger”.
         * Jako wejście otrzymasz spis 4 rozdziałów, które muszą pojawić się we wpisie (muszą zostać napisane przez LLM).
         * Jako odpowiedź musisz zwrócić tablicę (w formacie JSON) złożoną z 4 pól reprezentujących te cztery napisane rozdziały,
         * np.: {"answer":["tekst 1","tekst 2","tekst 3","tekst 4"]}
         */
        private static async Task<int> Blogger()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Blogger));

                var taskInput = AiDevsApiBaseFunctions.GetTask(client: client, token: token)?.blog;

                var answer = new string[taskInput.Count()];
                int i = 0;

                foreach (var input in taskInput)
                {
                    // TODO: żeby trzymał kontekst to trzeba dodawać do kolejnych zapytań całość rozmowy
                    var messagesTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat
                        (
                            systemMessage:
                                "You help to write a blog about a margherita pizza. " +
                                "Say what you know about the given title, like you were writing a blog about that title. " +
                                "Give a recipie if asked in any way for that. " +
                                "Answer as concisely as possible. " +
                                "Use polish language.",
                            userMessage: input
                        );


                    var gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: messagesTable);

                    Console.WriteLine($"{Environment.NewLine}-----------------------{Environment.NewLine}");
                    Console.WriteLine($"Rozdział:{Environment.NewLine}{input}{Environment.NewLine}");
                    Console.WriteLine($"Odpowiedź GPT:{Environment.NewLine}{gptAnswer}{Environment.NewLine}");
                    answer[i++] = gptAnswer;
                }

                AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: answer);
            }
            return await Task.FromResult(0);
        }

        /*
         * API: wykonaj zadanie o nazwie liar.
         * Jest to mechanizm, który mówi nie na temat w 1/3 przypadków.
         * Twoje zadanie polega na tym, aby do endpointa /task/ wysłać swoje pytanie w języku angielskim
         * (dowolne, np “What is capital of Poland?’) w polu o nazwie ‘question’
         * (metoda POST, jako zwykłe pole formularza, NIE JSON).
         * System API odpowie na to pytanie (w polu ‘answer’) lub zacznie opowiadać o czymś zupełnie innym, zmieniając temat.
         * Twoim zadaniem jest napisanie systemu filtrującego (Guardrails), który określi (YES/NO), czy odpowiedź jest na temat.
         * Następnie swój werdykt zwróć do systemu sprawdzającego jako pojedyncze słowo YES/NO.
         * Jeśli pobierzesz treść zadania przez API bez wysyłania żadnych dodatkowych parametrów, otrzymasz komplet podpowiedzi.
         * Skąd wiedzieć, czy odpowiedź jest ‘na temat’?
         * Jeśli Twoje pytanie dotyczyło stolicy Polski, a w odpowiedzi otrzymasz spis zabytków w Rzymie, to odpowiedź, którą należy wysłać do API to NO.
         */
        private static async Task<int> Liar()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Liar));

                string question = "What is capital of Poland?";

                var taskInput = AiDevsApiBaseFunctions.PostTask(client: client, token: token, question: question)?.answer;

                Console.WriteLine(
                    $"Question: {question}{Environment.NewLine}" +
                    $"Answer: {taskInput}{Environment.NewLine}");

                #region GuardRails

                var result = OpenAiApiFunctions.GuardRails_DoesAnswerAnswersTheQuestion(question: question, answer: taskInput);

                ChatCompletion chatCompletionObj = JsonConvert.DeserializeObject<ChatCompletion>(result);
                string gptAnswer = chatCompletionObj.Choices[0].Message.Content;

                if (gptAnswer != "YES" &&
                    gptAnswer != "NO")
                {
                    Console.WriteLine($"Problem, gpt nie ogarnął: {gptAnswer}");
                    return -1;
                }

                #endregion GuardRails

                AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: gptAnswer);
            }

            return await Task.FromResult(0);
        }

        #endregion C01

        #region C02

        /*
         * Skorzystaj z API tasks.aidevs.pl, aby pobrać dane zadania inprompt.
         * Znajdziesz w niej dwie właściwości —
         * input, czyli tablicę / listę zdań na temat różnych osób (każde z nich zawiera imię jakiejś osoby)
         * oraz question będące pytaniem na temat jednej z tych osób.
         * Lista jest zbyt duża, aby móc ją wykorzystać w jednym zapytaniu, więc dowolną techniką odfiltruj te zdania, które zawierają wzmiankę na temat osoby wspomnianej w pytaniu.
         * Ostatnim krokiem jest wykorzystanie odfiltrowanych danych jako kontekst na podstawie którego model ma udzielić odpowiedzi na pytanie.
         * Zatem:
         * pobierz listę zdań oraz pytanie,
         * skorzystaj z LLM, aby odnaleźć w pytaniu imię,
         * programistycznie lub z pomocą no-code odfiltruj zdania zawierające to imię.
         * Ostatecznie spraw by model odpowiedział na pytanie, a jego odpowiedź prześlij do naszego API w obiekcie JSON zawierającym jedną właściwość “answer”.
         */
        private static async Task<int> Inprompt()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Inprompt));

                var task = AiDevsApiBaseFunctions.GetTask(client: client, token: token);

                var question = task.question;

                #region wydobycie imienia z pytania

                var messagesTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                    systemMessage: "Get a name from the provided text. Write only the name, nothing else.",
                    userMessage: question);

                var gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(
                    messagesTable: messagesTable,
                    modelTemp: 0.1,
                    gptModel: GptModel.gpt40613);

                Console.WriteLine($"{Environment.NewLine}Pytanie: {question}{Environment.NewLine}Imię wyciągnięte przez chat: {gptAnswer}{Environment.NewLine}");

                #endregion wydobycie imienia z pytania

                #region wydobycie zdań tylko na temat tej osoby

                string inputsForTheName = string.Empty;

                foreach (var item in task.input)
                {
                    if (item.Contains(gptAnswer))
                    {
                        inputsForTheName += $"{item}{Environment.NewLine}";
                    }
                }

                Console.WriteLine($"Zdania na temat tej osoby:{Environment.NewLine}{inputsForTheName}");

                #endregion wydobycie zdań tylko na temat tej osoby

                messagesTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                    systemMessage: $"Context```{inputsForTheName}```",
                    userMessage: question);

                gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(
                    messagesTable: messagesTable,
                    modelTemp: 0.1,
                    gptModel: GptModel.gpt40613);

                Console.WriteLine($"{Environment.NewLine}Pytanie: {question}{Environment.NewLine}Odpowiedź GPT na pytanie: {gptAnswer}");

                AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: gptAnswer);
            }

            return await Task.FromResult(0);
        }

        /*
         * Korzystając z modelu text-embedding-ada-002 wygeneruj embedding dla frazy Hawaiian pizza — upewnij się, że to dokładnie to zdanie.
         * Następnie prześlij wygenerowany embedding na endpoint /answer.
         * Konkretnie musi być to format {"answer": [0.003750941, 0.0038711438, 0.0082909055, -0.008753223, -0.02073651, -0.018862579, -0.010596331, -0.022425512, ..., -0.026950065]}.
         * Lista musi zawierać dokładnie 1536 elementów.
         */
        private static async Task<int> Embedding()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Embedding));

                string textToEmbed = "Hawaiian pizza";

                var embedded = OpenAiApiFunctions.Embeddings_ReturnOnlyEmbedding(input: textToEmbed);

                AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: embedded);
            }

            return await Task.FromResult(0);
        }

        /*
         * Korzystając z modelu Whisper wykonaj zadanie API (zgodnie z opisem na tasks.aidevs.pl) o nazwie whisper.
         * W ramach zadania otrzymasz plik MP3 (15 sekund), który musisz wysłać do transkrypcji, a otrzymany z niej tekst odeślij jako rozwiązanie zadania.
         */
        private static async Task<int> Whisper()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Whisper));

                var taskMessage = AiDevsApiBaseFunctions.GetTask(client: client, token: token).message;

                var messagesTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                    systemMessage: "return only full file path from the provided text. Don't add any comments.",
                    userMessage: taskMessage);

                var audioUrl = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: messagesTable, modelTemp: 0.1, gptModel: GptModel.gpt40613);

                var whisperAnswer = OpenAiApiFunctions.Whisper_ReturnOnlyText(audioUrl: audioUrl);
                Console.WriteLine(whisperAnswer);

                AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: whisperAnswer);
            }
            return await Task.FromResult(0);
        }

        /*
         * Wykonaj zadanie o nazwie functions zgodnie ze standardem zgłaszania odpowiedzi opisanym na tasks.aidevs.pl.
         * Zadanie polega na zdefiniowaniu funkcji o nazwie addUser, która przyjmuje jako parametr obiekt z właściwościami:
         * imię (name, string), nazwisko (surname, string) oraz rok urodzenia osoby (year, integer).
         * Jako odpowiedź musisz wysłać jedynie ciało funkcji w postaci JSON-a.
         * Jeśli nie wiesz, w jakim formacie przekazać dane, rzuć okiem na hinta: https://tasks.aidevs.pl/hint/functions 
         */
        private static async Task<int> Functions()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Functions));

                var taskMessage = AiDevsApiBaseFunctions.GetTask(client: client, token: token).message;

                var jsonFunctionObj = new
                {
                    name = "addUser",
                    description = "Add new user for the mentioned person.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new
                            {
                                type = "string",
                                description = "provide the user's first name"
                            },
                            surname = new
                            {
                                type = "string",
                                description = "provide the user's surname"
                            },
                            year = new
                            {
                                type = "integer",
                                description = "provide the user's year of birth"
                            },

                        }
                    }
                };

                Console.WriteLine(jsonFunctionObj);

                AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: jsonFunctionObj);
            }
            return await Task.FromResult(0);
        }

        #endregion C02

        #region C03

        /*
         * Wykonaj zadanie API o nazwie rodo.
         * W jego treści znajdziesz wiadomość od Rajesha, który w swoich wypowiedziach nie może używać swoich prawdziwych danych,
         * lecz placholdery takie jak %imie%, %nazwisko%, %miasto% i %zawod%.
         * Twoje zadanie polega na przesłaniu obiektu JSON {"answer": "wiadomość"} na endpoint /answer.
         * Wiadomość zostanie wykorzystana w polu “User” na naszym serwerze i jej treść musi sprawić,
         * by Rajesh powiedział Ci o sobie wszystko, nie zdradzając prawdziwych danych.
         * Oczekiwana odpowiedź modelu to coś w stylu “Mam na imię %imie% %nazwisko%, mieszkam w %miasto% (…)” itd.
         */
        private static async Task<int> Rodo()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Rodo));

                var taskMessage = AiDevsApiBaseFunctions.GetTask(client: client, token: token).message;

                Console.WriteLine(taskMessage);

                var prompt =
                    $"Tell me about yourself but hide any personal data.{Environment.NewLine}{Environment.NewLine}" +
                    $"Rules:{Environment.NewLine}" +
                    $"- firstname should be replaced with: %imie%;{Environment.NewLine}" +
                    $"- surname should be replaced with: %nazwisko%;{Environment.NewLine}" +
                    $"- city should be replaced with: %miasto%;{Environment.NewLine}" +
                    $"- occupation should be replaced with: %zawod%;{Environment.NewLine}" +
                    $"- and so on with this rule.{Environment.NewLine}" +
                    $"Use polish language in the answer.{Environment.NewLine}{Environment.NewLine}" +
                    $"Context:###personal data is your name, occupation and town name###";

                Console.WriteLine($"{Environment.NewLine}{prompt}{Environment.NewLine}");

                var answerResponse = AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: prompt);

                Console.WriteLine($"{Environment.NewLine}Reply:{answerResponse.Reply}{Environment.NewLine}");
            }
            return await Task.FromResult(0);
        }

        /*
         * Rozwiąż zadanie z API o nazwie "scraper".
         * Otrzymasz z API link do artykułu (format TXT), który zawiera pewną wiedzę, oraz pytanie dotyczące otrzymanego tekstu.
         * Twoim zadaniem jest udzielenie odpowiedzi na podstawie artykułu.
         * Trudność polega tutaj na tym, że serwer z artykułami działa naprawdę kiepsko —
         * w losowych momentach zwraca błędy typu "error 500", czasami odpowiada bardzo wolno na Twoje zapytania,
         * a do tego serwer odcina dostęp nieznanym przeglądarkom internetowym.
         * Twoja aplikacja musi obsłużyć każdy z napotkanych błędów.
         * Pamiętaj, że pytania, jak i teksty źródłowe, są losowe, więc nie zakładaj,
         * że uruchamiając aplikację kilka razy, za każdym razem zapytamy Cię o to samo i będziemy pracować na tym samym artykule.
         */
        private static async Task<int> Scraper()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Scraper));

                var task = AiDevsApiBaseFunctions.GetTask(client: client, token: token);

                string websitePath = task.input;
                string question = task.question;

                Console.WriteLine($"{Environment.NewLine}Podpowiedź: {task.message}{Environment.NewLine}");
                Console.WriteLine($"Strona: {websitePath}{Environment.NewLine}");
                Console.WriteLine($"Pytanie: {question}{Environment.NewLine}");

                string contentOfTxt = string.Empty;
                bool succes = false;
                int i = 1;

                while (!succes)
                {
                    try
                    {
                        Console.WriteLine($"Kolejna próba pobrania informacji...");
                        var request = (HttpWebRequest)WebRequest.Create(websitePath);
                        // ile minut * ile sekund * 1000 milisekund
                        request.Timeout = 100;
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.3";

                        var response = request.GetResponse();
                        var stream = response.GetResponseStream();
                        var reader = new StreamReader(stream);
                        contentOfTxt = reader.ReadToEnd();
                        succes = true;
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = ConsoleColor.White;

                        // ile sekund * 1000 = milisekundy
                        System.Threading.Thread.Sleep(i * 1000);
                        Console.WriteLine($"Oczekujemy {i} sekund...");
                        i *= 2;
                    }
                }

                Console.WriteLine(contentOfTxt);

                var messagesTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                    systemMessage:
                        $"{task.Message}" +
                        $"Article```{Environment.NewLine}{contentOfTxt}{Environment.NewLine}```",
                    userMessage: $"{websitePath}{Environment.NewLine}{question}");

                var gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: messagesTable);

                Console.WriteLine($"{Environment.NewLine}{gptAnswer}{Environment.NewLine}");

                var answerResponse = AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: gptAnswer);

                Console.WriteLine(
                    $"Reply:{Environment.NewLine}{answerResponse.Reply}{Environment.NewLine}" +
                    $"Message:{answerResponse.Message}{Environment.NewLine}" +
                    $"Note:{answerResponse.Note}{Environment.NewLine}");
            }
            return await Task.FromResult(0);
        }

        /*
         * Rozwiąż zadanie o nazwie “whoami”.
         * Za każdym razem, gdy pobierzesz zadanie, system zwróci Ci jedną ciekawostkę na temat pewnej osoby.
         * Twoim zadaniem jest zbudowanie mechanizmu, który odgadnie, co to za osoba.
         * W zadaniu chodzi o utrzymanie wątku w konwersacji z backendem.
         * Jest to dodatkowo utrudnione przez fakt, że token ważny jest tylko 2 sekundy (trzeba go cyklicznie odświeżać!).
         * Celem zadania jest napisania mechanizmu, który odpowiada, czy na podstawie otrzymanych hintów jest w stanie powiedzieć,
         * czy wie, kim jest tajemnicza postać. Jeśli odpowiedź brzmi NIE, to pobierasz kolejną wskazówkę i doklejasz ją do bieżącego wątku.
         * Jeśli odpowiedź brzmi TAK, to zgłaszasz ją do /answer/.
         * Wybraliśmy dość ‘ikoniczną’ postać, więc model powinien zgadnąć, o kogo chodzi, po maksymalnie 5-6 podpowiedziach.
         * Zaprogramuj mechanizm tak, aby wysyłał dane do /answer/ tylko, gdy jest absolutnie pewny swojej odpowiedzi.
         */
        private static async Task<int> Whoami()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                bool gptIsSure = false;
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Whoami));

                int aiDevsRequestsCountPer10Sec = 1;

                string taskHint = AiDevsApiBaseFunctions.GetTask(client: client, token: token).hint;

                var gptConversation = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                    systemMessage: $"Spróbuj odgadnąć o kim mowa po podanych wskazówkach.{Environment.NewLine}{Environment.NewLine}" +
                        $"Zasady:{Environment.NewLine}" +
                        $"- dopóki nie jesteś pewien o kim mowa to odpowiadaj 'NIE';{Environment.NewLine}" +
                        $"- jeśli jesteś pewien o kim mowa, powiedz 'TAK';{Environment.NewLine}" +
                        $"- jeśli zostanie zadane pytanie, odpowiedz na nie zwięźle.{Environment.NewLine}{Environment.NewLine}" +
                        $"PAMIĘTAJ: dodawanie jakichkolwiek komentarzy jest zabronione.{Environment.NewLine}",
                    userMessage: taskHint);

                string gptResponse;

                while (!gptIsSure)
                {
                    if (aiDevsRequestsCountPer10Sec == 4)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Oczekiwanie 10 sekund, aby nie przekroczyć limitu...");
                        Console.ForegroundColor = ConsoleColor.White;
                        System.Threading.Thread.Sleep(10 * 1000);
                        aiDevsRequestsCountPer10Sec = 0;
                    }
                    token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Whoami));
                    taskHint = AiDevsApiBaseFunctions.GetTask(client: client, token: token).hint;
                    aiDevsRequestsCountPer10Sec++;
                    Console.WriteLine($"Wskazówka: {taskHint}");

                    OpenAiApiBaseFunctions.AddNewEntryToTheConversation(
                        existingConversation: ref gptConversation,
                        role: "user",
                        msg: taskHint);

                    gptResponse = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: gptConversation, gptModel: GptModel.gpt40613);
                    Console.WriteLine($"Odpowiedź GPT: {gptResponse}");

                    OpenAiApiBaseFunctions.AddNewEntryToTheConversation(
                        existingConversation: ref gptConversation,
                        role: "assistant",
                        msg: gptResponse);

                    if (gptResponse == "TAK")
                    {
                        gptIsSure = true;
                    }
                    else if (gptResponse == "NIE")
                    {
                        // nic
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Wykryto odpowiedź modelu spoza zasad: {gptResponse}");
                        Console.ForegroundColor = ConsoleColor.White;
                        return await Task.FromResult(0);
                    }
                }

                OpenAiApiBaseFunctions.AddNewEntryToTheConversation(
                    existingConversation: ref gptConversation,
                    role: "user",
                    msg: "O kim mowa? Podaj tylko imię i nazwisko, nic więcej.");

                gptResponse = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: gptConversation);
                Console.WriteLine($"Odgadnięcie GPT: {gptResponse}");

                var answerResponse = AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: gptResponse);

                Console.WriteLine($"{Environment.NewLine}Reply:{answerResponse.Reply}{Environment.NewLine}");
            }
            return await Task.FromResult(0);
        }

        /*
         * Dziś zadanie jest proste, ale nie łatwe — zaimportuj do swojej bazy wektorowej, spis linków z newslettera unknowNews z adresu:
         * https://unknow.news/archiwum_aidevs.json
         * [to mały wycinek bazy, jeśli chcesz pobrać całą bazę, to użyj pliku archiwum.json]
         * Następnie wykonaj zadanie API o nazwie “search” —
         * odpowiedz w nim na zwrócone przez API pytanie.
         * Odpowiedź musi być adresem URL kierującym do jednego z linków unknowNews. Powodzenia! 
         */
        private static async Task<int> Search()
        {
            using (var aiDevsClient = AiDevsApiBaseFunctions.CreateClient())
            {
                var link = @"https://unknow.news/archiwum_aidevs.json";
                string collName = "unknownNews";

                await ImportJsonToQdrant<UnknownNewsObj>(jsonUrl: link, collName: collName);

                var token = AiDevsApiBaseFunctions.Authorize(client: aiDevsClient, taskName: nameof(Search));
                string taskQuestion = AiDevsApiBaseFunctions.GetTask(client: aiDevsClient, token: token).question;

                Console.WriteLine($"Pytanie: {taskQuestion}");

                var embeddedQuestion = OpenAiApiFunctions.Embeddings_ReturnOnlyEmbedding(input: taskQuestion, gptModel: GptModel.textembedding3small).ToArray();

                var jsonSearch = JsonConvert.SerializeObject(new
                {
                    vector = embeddedQuestion,
                    limit = 3,
                    with_payload = true,
                });

                var result = await QdrantApiFunctions.Search(collectionName: collName, json: jsonSearch);

                var payload = result.Results.FirstOrDefault().Payload;

                Console.WriteLine($"Title: {payload.Title}{Environment.NewLine}Url: {payload.url}");

                AiDevsApiBaseFunctions.Answer(client: aiDevsClient, token: token, answer: payload.Url);
            }

            return await Task.FromResult(0);
        }

        /*
         * Rozwiąż zadanie o nazwie “people”.
         * Pobierz, a następnie zoptymalizuj odpowiednio pod swoje potrzeby bazę danych https://tasks.aidevs.pl/data/people.json.
         * Twoim zadaniem jest odpowiedź na pytanie zadane przez system.
         * Uwaga! Pytanie losuje się za każdym razem na nowo, gdy odwołujesz się do /task.
         * Spraw, aby Twoje rozwiązanie działało za każdym razem, a także, aby zużywało możliwie mało tokenów.
         * Zastanów się, czy wszystkie operacje muszą być wykonywane przez LLM-a - może warto zachować jakiś balans między światem kodu i AI?
         */
        private static async Task<int> People()
        {
            using (var aiDevsClient = AiDevsApiBaseFunctions.CreateClient())
            {
                var link = @"https://tasks.aidevs.pl/data/people.json";
                string collName = "people";

                await ImportJsonToQdrant<PeopleObj>(jsonUrl: link, collName: collName);

                var token = AiDevsApiBaseFunctions.Authorize(client: aiDevsClient, taskName: nameof(People));
                string taskQuestion = AiDevsApiBaseFunctions.GetTask(client: aiDevsClient, token: token).question;

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Pytanie: {taskQuestion}{Environment.NewLine}");
                Console.ForegroundColor = ConsoleColor.White;

                string gptAnswer = string.Empty;
                string jsonSearch = string.Empty;
                string methodName = "search";

                using (var openAiClient = OpenAiApiBaseFunctions.CreateClient())
                {
                    var msgTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                        systemMessage:
                            $"Dostaniesz pytanie. " +
                            $"Jeśli pytanie zawiera jakiekolwiek imię i nazwisko, wygeneruj JSON imieniem i nazwiskiem tej osoby. Jeśli imię jest zdrobnione, zamień je na pełną wersję tego imienia.{Environment.NewLine}" +
                            $"Jeśli pytanie nie zawiera nazwiska, odpowiedz tylko 'pytanie'." +
                            $"###{Environment.NewLine}Przykłady:{Environment.NewLine}{Environment.NewLine}" +
                            $"- Ile lat ma Ania Nowak?{Environment.NewLine}" +
                            @"- { ""imie"": ""Anna"", ""nazwisko"": = ""Nowak"" }" +
                            $"{Environment.NewLine}{Environment.NewLine}" +
                            $"- Jaki sport lubi Adrian Nowacki?{Environment.NewLine}" +
                            @"- { ""imie"": ""Adrian"", ""nazwisko"": ""Nowacki"" }" +
                            $"{Environment.NewLine}{Environment.NewLine}" +
                            $"- Kto lubi piłkę nożną?{Environment.NewLine}" +
                            $"- pytanie{Environment.NewLine}###",
                        userMessage: taskQuestion);

                    gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(msgTable, gptModel: GptModel.gpt4turbo);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(gptAnswer);
                    Console.ForegroundColor = ConsoleColor.White;

                    bool questionStrictlyAsked = false;

                    if (gptAnswer.ToLower().Contains("pytanie"))
                    {
                        var embeddedQuestion = OpenAiApiFunctions.Embeddings_ReturnOnlyEmbedding(input: taskQuestion, gptModel: GptModel.textembedding3small).ToArray();

                        jsonSearch = JsonConvert.SerializeObject(new
                        {
                            vector = embeddedQuestion,
                            limit = 1,
                            with_payload = true,
                        });

                        questionStrictlyAsked = true;
                    }
                    else
                    {
                        var jsonGpt = JsonConvert.DeserializeObject<dynamic>(gptAnswer);

                        methodName = "scroll";

                        jsonSearch = JsonConvert.SerializeObject(new
                        {
                            filter = new
                            {
                                must = new[]
                                {
                                new { key = "imie", match = new { value = jsonGpt.imie } },
                                new { key = "nazwisko", match = new { value = jsonGpt.nazwisko } },
                                }
                            }
                        });
                    }

                    var result = await QdrantApiFunctions.Search(collectionName: collName, json: jsonSearch, methodName);
                    var payload = result.result.points[0].payload;

                    if (payload == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Nikogo nie odnaleziono!");
                        Console.ForegroundColor = ConsoleColor.White;
                        return await Task.FromResult(-1);
                    }

                    Console.WriteLine($"Znaleziona osoba: {payload.imie} {payload.nazwisko}: {payload.informacje}{Environment.NewLine}");

                    string finalAnswer = string.Empty;

                    if (questionStrictlyAsked)
                    {
                        finalAnswer = $"{payload.imie} {payload.nazwisko}";
                    }
                    else
                    {
                        msgTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                            systemMessage:
                                $"Odpowiedz możliwie najkrócej na pytanie, bazując na poniższej informacji. Jeśli w informacji nie ma odpowiedzi na pytanie, odpowiedz tylko 'ERROR'{Environment.NewLine}" +
                                $"###{Environment.NewLine}{payload.imie} {payload.nazwisko}: {payload.informacje}{Environment.NewLine}###",
                            userMessage: taskQuestion);

                        finalAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(msgTable, gptModel: GptModel.gpt4turbo);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(finalAnswer);
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    AiDevsApiBaseFunctions.Answer(client: aiDevsClient, token: token, answer: finalAnswer);
                }
            }
            return await Task.FromResult(0);
        }

        #endregion C03

        #region C04

        /*
         * Wykonaj zadanie API o nazwie ‘knowledge’.
         * Automat zada Ci losowe pytanie na temat kursu walut, populacji wybranego kraju lub wiedzy ogólnej.
         * Twoim zadaniem jest wybór odpowiedniego narzędzia do udzielenia odpowiedzi (API z wiedzą lub skorzystanie z wiedzy modelu).
         * W treści zadania uzyskanego przez API, zawarte są dwa API, które mogą być dla Ciebie użyteczne.
         * Jeśli zwracasz liczbę w odpowiedzi, to zadbaj, aby nie miała ona zbytecznego formatowania (✅ 1234567, ❌ 1 234 567).
         */
        private static async Task<int> Knowledge()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Knowledge));
                var task = AiDevsApiBaseFunctions.GetTask(client: client, token: token);
                string database1 = task["database #1"];
                string database2 = task["database #2"];
                string question = task.question;

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{task.msg}{Environment.NewLine}");
                Console.WriteLine($"Pytanie: {question}{Environment.NewLine}");
                Console.WriteLine($"Databse #1: {database1}{Environment.NewLine}");
                Console.WriteLine($"Databse #2: {database2}{Environment.NewLine}");
                Console.ForegroundColor = ConsoleColor.White;

                using (var openAiClient = OpenAiApiBaseFunctions.CreateClient())
                {
                    var msgTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                        systemMessage: 
                            $"You will be provided with a question in polish language. " +
                            $"If the question is about any country's population, say only 'COUNTRY'. " +
                            $"If the question is about currency, say only 'CURRENCY'. " +
                            $"If none of that, answer the question ultra-concisely.",
                        userMessage: question);

                    var gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: msgTable, gptModel: GptModel.gpt35turbo);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Odpowiedź GPT: {gptAnswer}");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (gptAnswer.ToUpper().Contains("CURRENCY"))
                    {
                        msgTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                            systemMessage:
                                $"Say only the mentioned currency three- letter code (ISO 4217 standard), nothing else.",
                            userMessage: question);

                        var currency = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: msgTable, gptModel: GptModel.gpt35turbo);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Jaka waluta wg. GPT: {currency}");
                        Console.ForegroundColor = ConsoleColor.White;

                        using (var currencyClient = new HttpClient())
                        {
                            var uri = new Uri("http://api.nbp.pl/");
                            var response = await currencyClient.GetStringAsync($"{uri}api/exchangerates/rates/A/{currency}/");
                            var answer = JsonConvert.DeserializeObject<dynamic>(response).rates[0].mid;
                            gptAnswer = answer.ToString();
                        }
                    }
                    else if (gptAnswer.ToUpper().Contains("COUNTRY"))
                    {
                        msgTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                            systemMessage:
                                $"Say only the mentioned country's name in english, nothing else.",
                            userMessage: question);

                        var country = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: msgTable, gptModel: GptModel.gpt35turbo);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Jakie państwo wg. GPT: {country}");
                        Console.ForegroundColor = ConsoleColor.White;

                        using (var countryClient = new HttpClient())
                        {
                            var uri = new Uri("https://restcountries.com/v3.1/name/");
                            var response = await countryClient.GetStringAsync($"{uri}{country}");
                            var data = JsonConvert.DeserializeObject<dynamic>(response);
                            var population = data[0].population;
                            gptAnswer = population.ToString();
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Odpowiedź ostateczna: {gptAnswer}");
                    Console.ForegroundColor = ConsoleColor.White;

                    AiDevsApiBaseFunctions.Answer(
                        client: client,
                        token: token,
                        answer: gptAnswer);
                }
            }
            return await Task.FromResult(0);
        }

        /*
         * Rozwiąż zadanie API o nazwie ‘tools’.
         * Celem zadania jest zdecydowanie, czy podane przez API zadanie powinno zostać dodane do listy zadań (ToDo),
         * czy do kalendarza (jeśli ma ustaloną datę).
         * Oba narzędzia mają lekko różniące się od siebie definicje struktury JSON-a (różnią się jednym polem).
         * Spraw, aby Twoja aplikacja działała poprawnie na każdym zestawie danych testowych.
         */
        private static async Task<int> Tools()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Tools));
                string taskQuestion = AiDevsApiBaseFunctions.GetTask(client: client, token: token).question;

                using (var openAiClient = OpenAiApiBaseFunctions.CreateClient())
                {
                    var msgTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                        systemMessage:
                            $"Describe the message as either a 'ToDo' or a 'Calendar', add a short description of what should've been done and add a date if specified (even in realtive format like: Tomorrow). " +
                            $"Return the JSON in the format shown in the examples.{Environment.NewLine}" +
                            $"Rules###{Environment.NewLine}" +
                            $"- 'Calendar' will be anything that has a date specified{Environment.NewLine}." +
                            $"- 'ToDo' will be anything with no date specified.{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            $"Examples###{Environment.NewLine}" +
                            $"- Przypomnij mi, że mam kupić mleko{Environment.NewLine}" +
                            $"- {{\"tool\":\"ToDo\",\"desc\":\"Kup mleko\" }}" +
                            $"- Jutro mam spotkanie z Marianem{Environment.NewLine}" +
                            $"- {{\"tool\":\"Calendar\", \"desc\":\"Spotkanie z Marianem\", \"date\":\"2024-05-15\"}}{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            $"Context###{Environment.NewLine}" +
                            $"- today is 2024-05-14",
                        userMessage: taskQuestion);

                    var gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: msgTable, gptModel: GptModel.gpt4turbo);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Odpowiedź GPT: {gptAnswer}{Environment.NewLine}");
                    Console.ForegroundColor = ConsoleColor.White;

                    JObject obj = JObject.Parse(gptAnswer);

                    string tool = (string)obj["tool"]; // "Calendar"
                    string desc = (string)obj["desc"]; // "Urodziny Zenona"
                    string date = (string)obj["date"]; // "2024-05-20"

                    var actionToDo = new ActionToDo()
                    {
                        tool = tool,
                        desc = desc,
                        date = date,
                    };

                    AiDevsApiBaseFunctions.Answer(
                        client: client,
                        token: token,
                        answer: actionToDo);
                }
            }

            return await Task.FromResult(0);
        }

        public class ActionToDo
        {
            [JsonProperty(PropertyName = "tool")]
            [DataMember(Name = "tool")]
            public string tool
            {
                get;
                set;
            }
            [JsonProperty(PropertyName = "desc")]
            [DataMember(Name = "desc")]
            public string desc
            {
                get;
                set;
            }
            [JsonProperty(PropertyName = "date")]
            [DataMember(Name = "date")]
            public string date
            {
                get;
                set;
            }
        }

        /*
         * Rozwiąż zadanie API o nazwie ‘gnome’.
         * Backend będzie zwracał Ci linka do obrazków przedstawiających gnomy/skrzaty.
         * Twoim zadaniem jest przygotowanie systemu, który będzie rozpoznawał, jakiego koloru czapkę ma wygenerowana postać.
         * Uwaga! Adres URL zmienia się po każdym pobraniu zadania i nie wszystkie podawane obrazki zawierają zdjęcie postaci w czapce.
         * Jeśli natkniesz się na coś, co nie jest skrzatem/gnomem, odpowiedz “error”. Do tego zadania musisz użyć GPT-4V (Vision).
         */
        private static async Task<int> Gnome()
        {
            using (var aiDevsClient = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: aiDevsClient, taskName: nameof(Gnome));
                var task = AiDevsApiBaseFunctions.GetTask(client: aiDevsClient, token: token);
                string url = task.url;

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Msg: {task.message}");
                Console.WriteLine($"Hint: {task.hint}");
                Console.WriteLine($"Url: {url}");
                Console.ForegroundColor = ConsoleColor.White;

                var messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new dynamic[]
                        {
                            new
                            {
                                type = "text",
                                text = "Is there a gnome on the image? If the answer is NO, say only 'ERROR'. If the answer is YES, say only what hat color does it have, in polish langauge."
                            },
                            new
                            {
                                type = "image_url",
                                image_url = new { url = url, }
                            }
                        }
                    }
                };

                var gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: messages, gptModel: GptModel.gpt4turbo);

                Console.WriteLine(gptAnswer);

                var answerResponse = AiDevsApiBaseFunctions.Answer(
                    client: aiDevsClient,
                    token: token,
                    answer: gptAnswer);
            }

            return await Task.FromResult(0);
        }

        /*
         * Rozwiąż zadanie API o nazwie ‘ownapi’.
         * Zadanie polega na zbudowaniu prostego API webowego,
         * które będzie odpowiadać na pytania zadawane przez nasz system sprawdzania zadań.
         * Adres URL do API działającego po HTTPS prześlij w polu ‘answer’ do endpointa /answer/.
         * System na wskazany adres URL wyśle serię pytań (po jednym na żądanie).
         * Swoje API możesz hostować na dowolnej platformie no-code (Make/N8N),
         * jak i na własnym serwerze VPS, czy hostingu współdzielonym.
         * Możesz też hostowac to API na własnym komputerze i wystawić nam je np. za pomocą usługi NGROK.
         * Jeśli nadal komunikacja z API jest niejasna, przeczytaj hinta https://tasks.aidevs.pl/hint/ownapi
         */
        private static async Task<int> Ownapi()
        {
            return await Task.FromResult(0);
        }

        /*
         * Rozwiąż zadanie API o nazwie “ownapipro”.
         * Jak nazwa wskazuje, jest to rozbudowana wersja wczorajszego zadania (możesz więc wykorzystać wczoraj napisany kod!).
         * Przebieg zadania jest identyczny jak w przypadku ownapi,
         * z tą tylko różnicą, że system sprawdzający nie zawsze zadaje pytania,
         * a czasami przekazuje informacje, które musisz zapamiętać.
         * Przykładowo, podczas jednej rozmowy system przekaże informację „Mieszkam w Krakowie”,
         * a w kolejnej zapyta “Gdzie mieszkam?”.
         * Musisz więc obsłużyć zarówno wiedzę ogólną modelu, jak i wiedzę przekazaną do modelu w trakcie rozmowy (pamięć stała).
         * Utrudnieniem jest fakt, że dane przekazywane do zapamiętania, zmieniają się przy każdym uruchomieniu testu.
         * Dodatkowe hinty do zadania https://tasks.aidevs.pl/hint/ownapipro
         */
        private static async Task<int> Ownapipro()
        {
            return await Task.FromResult(0);
        }

        #endregion C04

        #region C05

        /*
         * Wykonaj zadanie API o nazwie “meme”.
         * Celem zadania jest nauczenie Cię pracy z generatorami grafik i dokumentów.
         * Zadanie polega na wygenerowaniu mema z podanego obrazka i podanego tekstu.
         * Mem ma być obrazkiem JPG o wymiarach 1080x1080.
         * Powinien posiadać czarne tło, dostarczoną grafikę na środku i podpis zawierający dostarczony tekst.
         * Grafikę z memem możesz wygenerować za pomocą darmowych tokenów dostępnych w usłudze RenderForm (50 pierwszych grafik jest darmowych).
         * URL do wygenerowanej grafiki spełniającej wymagania wyślij do endpointa /answer/.
         * W razie jakichkolwiek problemów możesz sprawdzić hinty https://tasks.aidevs.pl/hint/meme 
         * Ideą jest pokazanie Ci prostej metody na generowanie obrazów i dokumentów (np. PDF) z użyciem technologii no-code.
         * Może to być np. użyteczne przy generowaniu faktur, certyfikatów itp.
         * Ta wiedza z pewnością Ci się przy automatyzacji procesów biznesowych.
         */
        private static async Task<int> Meme()
        {
            using (var aiDevsClient = AiDevsApiBaseFunctions.CreateClient())
            {
                var token = AiDevsApiBaseFunctions.Authorize(client: aiDevsClient, taskName: nameof(Meme));
                var task = AiDevsApiBaseFunctions.GetTask(client: aiDevsClient, token: token);

                string imageUrl = task.image;
                string text = task.text;

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Image url: {imageUrl}");
                Console.WriteLine($"Text: {text}");
                Console.ForegroundColor = ConsoleColor.White;

                string memeUrl = string.Empty;

                using (HttpClient renderFormClient = new HttpClient())
                {
                    renderFormClient.BaseAddress = new Uri(@"https://get.renderform.io/api/v2/render");
                    renderFormClient.DefaultRequestHeaders.Add("X-API-KEY", "key-nWJl7wr1ufTxks7mEWVpbo1xy8aj7oWY3b");

                    var payload = new Dictionary<string, object>
                    {
                        { "template", "dashing-geckos-kneel-boldly-1143" },
                        { "data", new Dictionary<string, string>
                          {
                              { "textid.text", text },
                              { "imageid.src", imageUrl }
                          }
                        }
                    };

                    var json = JsonConvert.SerializeObject(payload);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await renderFormClient.PostAsync("https://get.renderform.io/api/v2/render", content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error message: {responseObject.msg}");
                        Console.ForegroundColor = ConsoleColor.White;
                        return await Task.FromResult(-1);
                    }

                    memeUrl = responseObject.href;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Url wygenerowanego mema: {memeUrl}");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                var answerResponse = AiDevsApiBaseFunctions.Answer(client: aiDevsClient, token: token, answer: memeUrl);
            }

            return await Task.FromResult(0);
        }

        /*
         * Rozwiąż zadanie API o nazwie ‘optimaldb’.
         * Masz dostarczoną bazę danych o rozmiarze ponad 30kb.
         * https://tasks.aidevs.pl/data/3friends.json 
         * Musisz zoptymalizować ją w taki sposób, aby automat korzystający z niej,
         * a mający pojemność pamięci ustawioną na 9kb był w stanie odpowiedzieć na 6 losowych pytań
         * na temat trzech osób znajdujących się w bazie.
         * Zoptymalizowaną bazę wyślij do endpointa /answer/ jako zwykły string.
         * Automat użyje jej jako fragment swojego kontekstu i spróbuje odpowiedzieć na pytania testowe.
         * Wyzwanie polega na tym, aby nie zgubić żadnej informacji
         * i nie zapomnieć kogo ona dotyczy oraz aby zmieścić się w wyznaczonym limicie rozmiarów bazy.
         */
        private static async Task<int> Optimaldb()
        {
            using (var client = AiDevsApiBaseFunctions.CreateClient())
            {
                var link = @"https://tasks.aidevs.pl/data/3friends.json";

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, link);
                var result = client.SendAsync(req).Result.Content;
                var response = result.ReadAsStringAsync().Result;
                var jsonResponse = JsonConvert.SerializeObject(response);

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{response}{Environment.NewLine}");
                Console.ForegroundColor = ConsoleColor.White;

                JObject jsonObject = JObject.Parse(response);
                string firstAnswer = string.Empty;
                string finalAnswer = string.Empty;
                int i = 0;

                using (var openAiClient = OpenAiApiBaseFunctions.CreateClient())
                {
                    //foreach (KeyValuePair<string, JToken> item in jsonObject)
                    //{
                    //    Console.WriteLine($"Name: {item.Key}");
                    //    string infoAboutThePerson = $"{Environment.NewLine}{item.Key}:";
                    //    JArray array = (JArray)item.Value;

                    //    foreach (JValue value in array)
                    //    {
                    //        Console.WriteLine($"Zdanie: {value.Value<string>()}");

                    //        var msgTable = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                    //            systemMessage:
                    //                $"Ultra-zwięźle podsumuj zdanie. Możesz rozdzielać zdanie na kilka, jeśli pozwoli to zmniejszyć ilość tokenów.{Environment.NewLine}" +
                    //                $"Zasady###{Environment.NewLine}" +
                    //                $"- masz zakaz używania imienia osoby, której tyczy się zdanie;" +
                    //                $"- pisz tak, jakby było wiadome, o kim mowa;{Environment.NewLine}" +
                    //                $"- pisz możliwie jak najkrócej;" +
                    //                $"- ograniczaj słowa- są na wagę złota;" +
                    //                $"- pisz ultra-zwięźle;" +
                    //                $"- pomijaj nieistotne słowa, które nie mają znaczenia dla informacji;" +
                    //                $"- jeśli się da podsumuj zdanie w stylu: 'favourite game: Tekken.'" +
                    //                $"- używaj języka angielskiego;" +
                    //                $"- po zakończeniu opisu sprawdź, czy na pewno nie ominąłeś żadnej informacji oraz czy napisałeś wszystko możliwie jak najkrócej. " +
                    //                    $"Przeanalizuj sobie to w głowie- nie dopisuj wniosków do odpowiedzi.{Environment.NewLine}" +
                    //                $"Przykład###{Environment.NewLine}" +
                    //                $"1.{Environment.NewLine}" +
                    //                $"INPUT: Kiedy zapytasz go o ulubioną grę planszową, Zygfryd bez wahania odpowie, że jest nią 'Terra Mystica'.{Environment.NewLine}" +
                    //                $"WRONG OUTPUT: When asked about his favorite board game, Zygfryd will not hesitate to say that it is 'Terra Mystica'.{Environment.NewLine}" +
                    //                $"CORRECT OUTPUT: favourite board game: 'Terra Mystica'.{Environment.NewLine}" +
                    //                $"2.{Environment.NewLine}" +
                    //                $"INPUT: Kolekcji markowych zegarków Zygfryda mogłaby pozazdrościć niejedna osoba interesująca się zegarmistrzostwem." +
                    //                $"WRONG OUTPUT: Zygfryd's collection of branded watches could make many watch enthusiasts jealous." +
                    //                $"CORRECT OUTPUT: he has a large collection of branded watches.",
                    //            userMessage: value.Value<string>());

                    //        var gptAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: msgTable);

                    //        Console.WriteLine($"Podsumowanie: {gptAnswer}{Environment.NewLine}");

                    //        infoAboutThePerson += $"- {gptAnswer}{Environment.NewLine}";
                    //        i++;

                    //        //if (i % 10 == 0)
                    //        //{
                    //        //    Console.ReadKey();
                    //        //}
                    //    }

                    //    firstAnswer += $"{Environment.NewLine}{infoAboutThePerson}";
                    //}

                    firstAnswer =
                        $"zygfryd:- his program won an award for innovation in using JavaScript at the last tech conference.{Environment.NewLine}" +
                        $"- favourite musical instrument: ukulele. plays it at night after finishing coding for the day.{Environment.NewLine}" +
                        $"- he is also interested in cultivating houseplants, and among his collection, there is a rare species of orchid.{Environment.NewLine}" +
                        $"- logic puzzles are his passion; he often participates in competitions and achieves high rankings.{Environment.NewLine}" +
                        $"- favourite board game: 'Terra Mystica'.{Environment.NewLine}" +
                        $"- last year, he went on a hiking trip in the mountains and his Instagram is filled with photos from this journey.{Environment.NewLine}" +
                        $"- former elementary school spelling champion.{Environment.NewLine}" +
                        $"- winner of local programming marathon: mobile app designed by Zygfryd.{Environment.NewLine}" +
                        $"- favourite movie: 'Matrix'. He watches it at least twice a year.{Environment.NewLine}" +
                        $"- known in the Polish stand-up scene for his excellent sense of humor, although he only performs there as an amateur.{Environment.NewLine}" +
                        $"- favourite color: blue. It calms him down when coding complex algorithms.{Environment.NewLine}" +
                        $"- he leads culinary workshops promoting healthy eating despite loving pineapple pizza.{Environment.NewLine}" +
                        $"- favourite instrument: violin.{Environment.NewLine}" +
                        $"switched from violin lessons to computer passion.{Environment.NewLine}" +
                        $"- football fan: Zygfryd never misses his favorite team's matches.{Environment.NewLine}" +
                        $"- he has an enviable collection of branded watches.{Environment.NewLine}" +
                        $"- he spends at least half an hour every day learning a new foreign language - currently focusing on Spanish.{Environment.NewLine}" +
                        $"- one of the first programmers in Poland to start creating applications using the Vue.js framework.{Environment.NewLine}" +
                        $"- founder of the local programming club in his hometown.{Environment.NewLine}" +
                        $"- favourite dance for his wedding: classical tango, which he practiced for a long time.{Environment.NewLine}" +
                        $"- drink of choice: green tea.{Environment.NewLine}" +
                        $"- certificates in multiple programming languages.{Environment.NewLine}" +
                        $"- In his youth, Zygfryd played guitar in a rock band.{Environment.NewLine}" +
                        $"- favourite genre in his home library: science fiction, with 'Dune' by Franz Herbert at the top.{Environment.NewLine}" +
                        $"- he often goes on weekend bike trips to relax after a busy week of programming work.{Environment.NewLine}" +
                        $"- he proudly presents his martial arts skills, regularly practicing aikido.{Environment.NewLine}" +
                        $"- best gift for him: new tech gadget for work or hobby.{Environment.NewLine}" +
                        $"- favourite video games: strategic RPGs where he can showcase his planning skills.{Environment.NewLine}" +
                        $"- favourite hobby: modeling - he pays special attention to his model of the 'Enterprise' spaceship.{Environment.NewLine}" +
                        $"- he is a big fan of classic rock based on his vinyl record collection.{Environment.NewLine}" +
                        $"- he has an app on his phone that notifies him about upcoming International Space Station passes.{Environment.NewLine}" +
                        $"- he runs a blog sharing his experiences as a programming mentor.{Environment.NewLine}" +
                        $"- tech startup co-founded by Zygfryd recently won a significant industry award.{Environment.NewLine}" +
                        $"- he celebrates his name anniversary by organizing a big barbecue party for friends and family.{Environment.NewLine}" +
                        $"- he comes up with the most creative and original ideas during brainstorming sessions at work.{Environment.NewLine}" +
                        $"- He escapes to the wild regions of Poland every year to disconnect from the digital world despite being a technology enthusiast.{Environment.NewLine}" +
                        $"- he is fluent in sign language, which he learned to communicate with a colleague at work.{Environment.NewLine}" +
                        $"- he has a private wine cellar with selected bottles from around the world. his hobby is oenology.{Environment.NewLine}" +
                        $"- he is often asked for help in repairing old computers due to his skills in data recovery.{Environment.NewLine}" +
                        $"- he is talented in drawing and designing, evident in the graphics he prepares for his web applications.{Environment.NewLine}" +
                        $"- one of the best holidays in his life: Hawaii; his new passion is surfing.{Environment.NewLine}" +
                        $"- gained recognition for groundbreaking coding discovery; remains humble and helpful to others.{Environment.NewLine}" +
                        $"- excellent organizational skills, especially in coordinating complex software projects.{Environment.NewLine}" +
                        $"- He regularly publishes tips for beginner programmers on his JavaScript blog, sharing his own experiences.{Environment.NewLine}" +
                        $"- favourite hobby: developing indie games projects.{Environment.NewLine}" +
                        $"- he explores virtual reality to find new ways of using it in programming education.{Environment.NewLine}" +
                        $"- favourite comic book series: 'Watchmen' by Alan Moore.{Environment.NewLine}" +
                        $"- dreamed of becoming an astronaut as a child; now he develops software for the space industry.{Environment.NewLine}" +
                        $"- focuses on creating ethical algorithms for the benefit of society in his work on artificial intelligence.{Environment.NewLine}" +
                        $"- his obsession: searching for extraterrestrial life, reflected in his programming projects.{Environment.NewLine}" +
                        $"- preferred time to work: sunrise.{Environment.NewLine}" +
                        $"- he started popular coding workshops for children in his city, attracting more and more young talents.{Environment.NewLine}" +
                        $"- he built a drone one weekend and now uses it to film beautiful landscapes.{Environment.NewLine}" +
                        $"- created an open educational platform for programming.{Environment.NewLine}" +
                        $"- he sometimes talks about his grandfather, who was an inventor, sparking his fascination with technology.{Environment.NewLine}" +
                        $"- photography enthusiasts appreciate Zygfryd's photos showing unique perspectives of urban landscapes.{Environment.NewLine}" +
                        $"- he always tries to visit local art museums while traveling to broaden his aesthetic horizons.{Environment.NewLine}" +
                        $"- active involvement in charitable organizations helped raise funds for computers for a local school.{Environment.NewLine}" +
                        $"- he used to sing in an academic choir and has recordings of his solo performances.{Environment.NewLine}" +
                        $"- he prefers to create his own customized libraries tailored to specific project needs.{Environment.NewLine}" +
                        $"- he attends many programming meet-ups where he shares his thoughts on the future of the JavaScript language.{Environment.NewLine}" +
                        $"- his beloved cat often helps him out when he gets stuck in a programming project.{Environment.NewLine}" +
                        $"- interest in electronics and robotics sparked by a faulty kitchen robot.{Environment.NewLine}" +
                        $"- his fascination with astronomy is evident even in the naming of his programming projects, referencing cosmic terms.{Environment.NewLine}" +
                        $"- translated technical documentation into Polish, helping the local IT community.{Environment.NewLine}" +
                        $"- loyalty to open-source projects known among colleagues, often sought for advice.{Environment.NewLine}" +
                        $"- he placed in the top ten at a half marathon in his city, considering it a personal fitness success.{Environment.NewLine}" +
                        $"- he participated in archaeological excavations where previously unknown medieval artifacts were discovered.{Environment.NewLine}" +
                        $"- JavaScript expert; often invited to review books before their release.{Environment.NewLine}" +
                        $"- work experience: small start-ups and large IT corporations.{Environment.NewLine}" +
                        $"- he is also an amateur illusionist and can surprise friends with amazing card tricks.{Environment.NewLine}" +
                        $"- high empathy level makes him valued among colleagues as a support in difficult times.{Environment.NewLine}" +
                        $"- he has a mysterious hobby of collecting unique editions of old prints.{Environment.NewLine}" +
                        $"- he is a walking encyclopedia, with an amazing ability to memorize facts from various fields.{Environment.NewLine}" +
                        $"- side job: barista to improve latte art skills.{Environment.NewLine}" +
                        $"- has a keen sense of trends, invaluable for designing user interfaces that are always up to date.{Environment.NewLine}" +
                        $"- he helps young computer science enthusiasts by organizing coding courses after hours.{Environment.NewLine}" +
                        $"- created JavaScript library to improve website accessibility.{Environment.NewLine}" +
                        $"- he records his ideas and inspirations daily in a special creative journal.{Environment.NewLine}" +
                        $"- moonlight inspired him to design a mobile app for tracking lunar phases.{Environment.NewLine}" +
                        $"- dedicated wall in his office is full of demotivational posters with surprising and humorous quotes about programming.{Environment.NewLine}" +
                        $"- he is the life of the party, especially when it comes to technology-related events.{Environment.NewLine}" +
                        $"- favourite VR games: setting records and impressing friends with his skills.{Environment.NewLine}" +
                        $"{Environment.NewLine}" +
                        $"{Environment.NewLine}" +
                        $"stefan:- he organizes a contest for the longest eaten hot dog at Żabka store every Wednesday.{Environment.NewLine}" +
                        $"- After work at Żabka, Stefan regularly walks 5 kilometers to get to the gym.{Environment.NewLine}" +
                        $"- he can lift a barbell equal to his own weight.{Environment.NewLine}" +
                        $"- dream: open his own gym with hot dogs as an energy point.{Environment.NewLine}" +
                        $"- tattoo symbolizes his love for his childhood faithful dog.{Environment.NewLine}" +
                        $"- he allocates part of his hot dog sales income to support the local animal shelter.{Environment.NewLine}" +
                        $"- he always advises new customers at Żabka on the best sauce for a hot dog.{Environment.NewLine}" +
                        $"- participated in amateur bodybuilding competitions, finishing third in the beginners category.{Environment.NewLine}" +
                        $"- he creates a blog sharing tips on effective biceps workouts.{Environment.NewLine}" +
                        $"- he was named 'Salesperson of the Month' at Żabka stores five times in the last year.{Environment.NewLine}" +
                        $"- favourite food: hot dogs.{Environment.NewLine}" +
                        $"- During holidays, Stefan enjoys participating in culinary festivals, where he sells his signature hot dogs.{Environment.NewLine}" +
                        $"- special spice blend is the secret to Stefan's culinary popularity with hot dogs.{Environment.NewLine}" +
                        $"- plans to participate in national weightlifting competitions next year.{Environment.NewLine}" +
                        $"- helps Stefan monitor his progress at the gym.{Environment.NewLine}" +
                        $"- gifts for his birthday: gym and hot dog related gadgets.{Environment.NewLine}" +
                        $"- local hero: saved a cat stuck in a tree during a work break.{Environment.NewLine}" +
                        $"- favourite training gear: over 30 pairs of gloves.{Environment.NewLine}" +
                        $"- he experiments with new hot dog sauce recipes in his free time.{Environment.NewLine}" +
                        $"- nickname at the gym: 'Hot Dog King', related to his job and physique.{Environment.NewLine}" +
                        $"- important element of Stefan's holiday table: hot dogs with ham and horseradish, which he prepares himself.{Environment.NewLine}" +
                        $"- fastest at packing groceries among all employees.{Environment.NewLine}" +
                        $"- has a passion for sailing and dreams of creating a seafood hot dog.{Environment.NewLine}" +
                        $"- favourite documentaries: bodybuilders and successful salespeople.{Environment.NewLine}" +
                        $"- he helps organize local sports events when he is not working or training.{Environment.NewLine}" +
                        $"- excellent flavor balance in Stefan's hot dogs is a result of his culinary sense.{Environment.NewLine}" +
                        $"- he has a carefully curated set of weights for outdoor workouts.{Environment.NewLine}" +
                        $"- Stefan's hot dogs are always fresh and made with high-quality ingredients.{Environment.NewLine}" +
                        $"- favourite grill item: hot dogs. He is responsible for grilling during family gatherings.{Environment.NewLine}" +
                        $"- friends often ask him for advice on arm muscle building exercises.{Environment.NewLine}" +
                        $"- he uses his experience in hot dog sales to train younger Żabka store employees.{Environment.NewLine}" +
                        $"- impressive knowledge of German words related to hot dogs due to frequent trips to Germany.{Environment.NewLine}" +
                        $"- he works at Żabka and finds time to write a cookbook about deluxe hot dogs.{Environment.NewLine}" +
                        $"- Stefan participated in creating the longest hot dog in the city as part of a promotional campaign for the Żabka store.{Environment.NewLine}" +
                        $"- appeared on a local TV show about preparing the perfect hot dog.{Environment.NewLine}" +
                        $"- skill: sharing high-protein diet planning with other athletes.{Environment.NewLine}" +
                        $"- tattoo of a dachshund brings him luck at work and at the gym.{Environment.NewLine}" +
                        $"- favourite costume at costume parties: dachshund outfit, matching his tattoo.{Environment.NewLine}" +
                        $"- title: 'king of hot dogs', acknowledged with a smile.{Environment.NewLine}" +
                        $"- favourite activity: experimenting with thematic hot dog versions.{Environment.NewLine}" +
                        $"- favourite pastime during breaks: Stefan enjoys sharing trivia with clients about the origin of hot dog toppings.{Environment.NewLine}" +
                        $"- expert in choosing the right bun for a hot dog.{Environment.NewLine}" +
                        $"- he has qualifications for training beginners at the gym and often gives individual advice.{Environment.NewLine}" +
                        $"- During holiday festivals, Stefan serves hot dogs with thematic, colorful sauces.{Environment.NewLine}" +
                        $"- he has a well-trained physique from hard work, almost as much time as he spends working at Żabka.{Environment.NewLine}" +
                        $"- favourite topic with older Żabka customers: healthy lifestyle and benefits of physical activity.{Environment.NewLine}" +
                        $"- he is able to quickly calculate and give change to customers at Żabka.{Environment.NewLine}" +
                        $"- handmade paper decorations made by Stefan often adorn the hot dog product line at Żabka store.{Environment.NewLine}" +
                        $"- participates in the local competition 'Super Salesman' every year, achieving high rankings.{Environment.NewLine}" +
                        $"- he is always the reliable meal provider during local strength competitions.{Environment.NewLine}" +
                        $"- known for his personal approach and always smiling customer service.{Environment.NewLine}" +
                        $"- he has a first aid certificate, making him a potential rescuer in emergencies.{Environment.NewLine}" +
                        $"- Stefan often gets asked by children for dachshund-shaped balloons inspired by his tattoo.{Environment.NewLine}" +
                        $"- he runs a food stand with hot dogs during local health events.{Environment.NewLine}" +
                        $"- tried bodybuilding competitions, but chose a career as a salesman.{Environment.NewLine}" +
                        $"- he always considers the opinions of regular customers when creating new flavor compositions for hot dogs.{Environment.NewLine}" +
                        $"- his dream is to travel across the USA to uncover the secrets of the most famous hot dog recipes.{Environment.NewLine}" +
                        $"- he organizes small hot dog eating contests for customers at the Żabka store.{Environment.NewLine}" +
                        $"- he must have gym towels with dachshund prints in his bag, which entertains Stefan's friends.{Environment.NewLine}" +
                        $"- unique skill: Stefan can quickly make hot dog sausages from ready-made ground meat.{Environment.NewLine}" +
                        $"- he gives advice on nutrition and training for beginner bodybuilders on a local internet forum.{Environment.NewLine}" +
                        $"- he fills the fridge with more drinks on hot days for Żabka customers.{Environment.NewLine}" +
                        $"- he often experiments with exclusive versions of hot dogs.{Environment.NewLine}" +
                        $"- he rarely forgets to bring his own shaker to training because he regularly consumes protein shakes.{Environment.NewLine}" +
                        $"- the stability and rigidity of hot dogs sold by Stefan are often the subject of jokes at Żabka store.{Environment.NewLine}" +
                        $"- he has a collection of photos of various versions of hot dogs he prepared and sold over the years.{Environment.NewLine}" +
                        $"- favourite food: steak, especially after a hard workout.{Environment.NewLine}" +
                        $"- he uses social media for career development and shares his culinary creations.{Environment.NewLine}" +
                        $"- looking forward to food fair to present innovative hot dog recipes.{Environment.NewLine}" +
                        $"- he adapts his hot dog offerings to seasonal customer preferences.{Environment.NewLine}" +
                        $"- Stefan runs a booth at the urban food festival showcasing hot dogs and weightlifting demonstration.{Environment.NewLine}" +
                        $"- ensures there is enough ketchup and mustard for hot dogs at Żabka store.{Environment.NewLine}" +
                        $"- charity work is particularly close to Stefan's heart, often preparing free hot dogs for participants.{Environment.NewLine}" +
                        $"- favourite activity after strength training: listening to audiobooks on motivation and personal development.{Environment.NewLine}" +
                        $"- he shows his competitive spirit by participating in Żabka store sellers' competitions.{Environment.NewLine}" +
                        $"- specializes in local delicacies and often experiments with them to create unique versions of hot dogs.{Environment.NewLine}" +
                        $"{Environment.NewLine}" +
                        $"{Environment.NewLine}" +
                        $"ania:- During her law studies, Ania got involved in organizing a conference on copyright law.{Environment.NewLine}" +
                        $"- She runs a YouTube channel sharing beauty tips in her free time.{Environment.NewLine}" +
                        $"- summer internship at a prestigious law firm gains valuable experience.{Environment.NewLine}" +
                        $"- She showed amazing driving skills during the amateur Porsche Club races.{Environment.NewLine}" +
                        $"- her lifestyle: fitness; regularly competes in fitness bikini competitions.{Environment.NewLine}" +
                        $"- signature look: red lipstick, often showcased on social media.{Environment.NewLine}" +
                        $"- Ania discovered amazing culinary skills by preparing healthy and flavorful meals for her friends.{Environment.NewLine}" +
                        $"- significant part of Anna's student budget is allocated for new nail polish collections.{Environment.NewLine}" +
                        $"- she joined the university criminal law academic circle board to improve her organizational skills.{Environment.NewLine}" +
                        $"- Jennifer Lopez is her fitness inspiration, especially in dance and stage movement.{Environment.NewLine}" +
                        $"- hair accessories are Anna's obsession, which she proudly showcases on her Instagram.{Environment.NewLine}" +
                        $"- favourite literary genre: legal thriller.{Environment.NewLine}" +
                        $"- stands out on the ski slope with a professional and stylish sports outfit.{Environment.NewLine}" +
                        $"- She organizes self-defense workshops for women, emphasizing the importance of safety and assertiveness.{Environment.NewLine}" +
                        $"- she started volunteering at the local women's rights center.{Environment.NewLine}" +
                        $"- private joke: ANA911, personalized license plate for Porsche.{Environment.NewLine}" +
                        $"- favourite way to relax: spontaneous trips to the spa after a tough week at university.{Environment.NewLine}" +
                        $"- surprising Ania with new beauty products from subscription boxes every month.{Environment.NewLine}" +
                        $"- spent every summer doing legal internships in Paris, thanks to her fluent French.{Environment.NewLine}" +
                        $"- favourite movie genre: classic legal dramas. Night movie sessions are a way for her to relax.{Environment.NewLine}" +
                        $"- her completed medical law workshops expanded her interests to legal aspects of the cosmetics industry.{Environment.NewLine}" +
                        $"- successful in fitness competitions, invited to participate in a campaign promoting a healthy lifestyle.{Environment.NewLine}" +
                        $"- favourite activity: regular jogging in the park combining love for fitness and need for fresh air.{Environment.NewLine}" +
                        $"- involved in an academic project on cybersecurity.{Environment.NewLine}" +
                        $"- Ania always has a stylish leather notebook with her, both in lectures and at the gym.{Environment.NewLine}" +
                        $"- favourite skincare product: vitamin C serum.{Environment.NewLine}" +
                        $"- favourite game: Animal Crossing.{Environment.NewLine}" +
                        $"- symbol of patriotism and love for national symbols: white-tailed eagle adorning Ania's Porsche hood.{Environment.NewLine}" +
                        $"- Ania designed a winning legal bow that was incorporated into the architecture of the university law library.{Environment.NewLine}" +
                        $"- she takes care of her daily makeup by using a set of synthetic hair brushes that she carefully selected after a long search.{Environment.NewLine}" +
                        $"- favourite summer footwear: high-heeled openwork sandals, goes well with her dresses.{Environment.NewLine}" +
                        $"- favourite perfumes: limited editions of famous brands.{Environment.NewLine}" +
                        $"- she installed an advanced audio system in her Porsche to make every journey a musical adventure.{Environment.NewLine}" +
                        $"- hidden talent: crochet; creates unique accessories.{Environment.NewLine}" +
                        $"- favourite language: Spanish, passion: flamenco.{Environment.NewLine}" +
                        $"- she regularly visits the local animal shelter, offering volunteering and legal support.{Environment.NewLine}" +
                        $"- her speech on women's rights was highly popular with the audience.{Environment.NewLine}" +
                        $"- one of Ana's greatest achievements outside of the field of law: composing her own song on the guitar.{Environment.NewLine}" +
                        $"- Ania benefits from engaging in the Personal Branding Strong Project by combining the worlds of law and social media.{Environment.NewLine}" +
                        $"- favourite holiday: Halloween. Ania creates unique costumes using her makeup and dressing up skills.{Environment.NewLine}" +
                        $"- she meditates and goes jogging to focus and calm her mind before the exam.{Environment.NewLine}" +
                        $"- personal style: elegant briefcase collection.{Environment.NewLine}" +
                        $"- favourite activity during university breaks: participating in chess tournaments to improve strategic thinking.{Environment.NewLine}" +
                        $"- she tutors younger students in constitutional law, building her reputation as a lawyer.{Environment.NewLine}" +
                        $"- large back tattoo: combined symbols of rose and dove, holds deep personal meaning for her.{Environment.NewLine}";

                    var msgTable2 = OpenAiApiBaseFunctions.CreateBasicMessagesForChat(
                        systemMessage: "In a moment you will receive from me a database on three people. It is over 16kb in size. " +
                            "You need to prepare me for an exam in which I will be questioned on this database. " +
                            "Unfortunately, the capacity of my memory is just 9kb. Send me the optimised database. " +
                            "Do not loose ANY of the information, even if it doesn't look important. Favourite games, life goals, choosen wedding music, the inspirations- it's all important." +
                            "Do not shorten the database too much.",
                        //systemMessage: "You're a student and you need to write points list about 3 people for a teacher's task. Your basic points list is below. " +
                        //    "You need to shorten the points bu a half but without loosing any information, even if it doesn't look important. Favourite games, life goals, choosen wedding music- it's all important.",
                        userMessage: firstAnswer);

                    finalAnswer = OpenAiApiFunctions.Completions_ReturnOnlyGptAnswer(messagesTable: msgTable2, gptModel: GptModel.gpt4turbo);
                }

                Console.WriteLine($"Ostateczna odpowiedź: {finalAnswer}");

                var token = AiDevsApiBaseFunctions.Authorize(client: client, taskName: nameof(Optimaldb));
                string taskQuestion = AiDevsApiBaseFunctions.GetTask(client: client, token: token).question;

                AiDevsApiBaseFunctions.Answer(
                    client: client,
                    token: token,
                    answer: finalAnswer);
            }

            return await Task.FromResult(0);
        }

        /*
         * Rozwiąż zadanie API o nazwie ‘google’.
         * Do jego wykonania będziesz potrzebować darmowego konta w usłudze SerpAPI.
         * Celem zadania jest samodzielne zaimplementowanie rozwiązania podobnego do tego,
         * znanego z ChatGPT Plus, gdzie po wpisaniu zapytania na temat, o którym model nie ma pojęcia,
         * uruchamiana jest wyszukiwarka BING.
         * My posłużymy się wyszukiwarką Google, a Twój skrypt będzie wyszukiwał odpowiedzi 
         * na pytania automatu sprawdzającego i będzie zwracał je w czytelnej dla człowieka formie.
         * Więcej informacji znajdziesz w treści zadania /task/, a podpowiedzi dostępne są pod https://tasks.aidevs.pl/hint/google.
         */
        private static async Task<int> Google()
        {
            return await Task.FromResult(0);
        }

        /*
         * Rozwiąż zadanie API o nazwie ‘md2html’.
         * Twoim celem jest stworzenie za pomocą fine-tuningu modelu, który po otrzymaniu pliku Markdown na wejściu,
         * zwróci jego sformatowaną w HTML wersję.
         * Mamy tutaj jedno drobne utrudnienie, ponieważ znacznik pogrubienia jest konwertowany w bardzo nietypowy sposób.
         * Oto jak wygląda konwersja do HTML, którą chcemy otrzymać:
            
            # Nagłówek1 = <h1>Nagłówek1</h1>
            ## Nagłówek2 = <h2>Nagłówek2</h2>
            ### Nagłówek3= <h3>Nagłówek3</h3>
            **pogrubienie** = <span class="bold">pogrubienie</span>
            *kursywa* = <em>kursywa</em>
            [AI Devs 3.0](https://aidevs.pl) = <a href="https://aidevs.pl">AI Devs 3.0</a>
            _podkreślenie_ = <u>podkreślenie</u>
            
            Zaawansowana konwersja:
            1. Element listy
            2. Kolejny elementy
            
            Wynik:
            <ol>
            <li>Element listy</li>
            <li>Kolejny element</li>
            </ol>
            
         * Tekst otrzymany z endpointa /task/ musisz przepuścić przez swój, wytrenowany model,
         * a następnie zwrócić w standardowy sposób do /answer/.
         * Uwaga: do fine-tuningu użyj modelu gpt-3.5-turbo-0125.
         * Istnieje ogromna szansa, że przy pierwszych podejściach do nauki modelu otrzymasz bardzo częste halucynacje.
         * Zwiększ liczbę przykładów w pliku użytym do nauki i/lub pomyśl o zwiększeniu liczby cykli szkoleniowych.
         * To zadanie da się rozwiązać bez fine tuningu, ale zależy nam,
         * abyś jako kursant zaznajomił się z procesem uczenia modeli nowych umiejętności.
         * Dane wejściowe mogą zawierać zagnieżdżone znaczniki:
            
            # Bardzo _ważny_ tekst = <h1> Bardzo <u>ważny</u> tekst</h1>

         */
        private static async Task<int> Md2html()
        {
            return await Task.FromResult(0);
        }

        #endregion C05

    }
}