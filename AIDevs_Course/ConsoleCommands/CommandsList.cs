using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.ConsoleCommands;

namespace AIDevs_Course.ConsoleCommands
{
    public partial class CommandsList
    {

        public static readonly List<CommandAction> OpenAiCommandsList = new List<CommandAction>()
        {
            new CommandAction(key: "End", actionName: "zakończ program", action: End),
            new CommandAction(key: "1stMsg", actionName: "Pierwszy kontakt z GPT- podanie wiadomości systemowej i jednej wiadomości usera", action: FirstGptContact),
            new CommandAction(key: "GRQA", actionName: "GuardRails- czy odpowiedź odpowiada na zadane pytanie", action: GuardRails_DoesAnswerAnswersTheQuestion),
        };

        public static readonly List<CommandAction> QdrantCommandsList = new List<CommandAction>()
        {
            new CommandAction(key: "End", actionName: "zakończ program", action: End),
            new CommandAction(key: "WriteColls", actionName: "Write Collections Names - wypisz nazwy kolekcji", action: GetCollections),
            new CommandAction(key: "CreateColl", actionName: "Create Collection - dodaj kolekcję", action: CreateCollection),
            new CommandAction(key: "GetColl", actionName: "Get Collection - znajdź kolekcję", action: GetCollection),
            new CommandAction(key: "CreatePoints", actionName: "Create Points - dodaj wektory", action: CreatePoints),
        };

        public static readonly List<CommandAction> AiDevsCommandsList = new List<CommandAction>()
        {
            new CommandAction(key: "End", actionName: "zakończ program", action: End),

            // C01
            new CommandAction(key: "HelloApi", actionName: "Zadanie nr 1 (z C01 L01)", action: HelloApi),
            new CommandAction(key: "Moderation", actionName: "Zadanie nr 2 (z C01 L04)", action: Moderation),
            new CommandAction(key: "Blogger", actionName: "Zadanie nr 3 (z C01 L04)", action: Blogger),
            new CommandAction(key: "Liar", actionName: "Zadanie nr 4 (z C01 L05)", action: Liar),

            // C02
            new CommandAction(key: "Inprompt", actionName: "Zadanie nr 5 (z C02 L02)", action: Inprompt),
            new CommandAction(key: "Embedding", actionName: "Zadanie nr 6 (z C02 L03)", action: Embedding),
            new CommandAction(key: "Whisper", actionName: "Zadanie nr 7 (z C02 L04)", action: Whisper),
            new CommandAction(key: "Functions", actionName: "Zadanie nr 8 (z C02 L05)", action: Functions),

            // C03
            new CommandAction(key: "Rodo", actionName: "Zadanie nr 9 (z C03 L01)", action: Rodo),
            new CommandAction(key: "Scraper", actionName: "Zadanie nr 10 (z C03 L02)", action: Scraper),
            new CommandAction(key: "Whoami", actionName: "Zadanie nr 11 (z C03 L03)", action: Whoami),
            new CommandAction(key: "Search", actionName: "Zadanie nr 12 (z C03 L04)", action: Search),
            new CommandAction(key: "People", actionName: "Zadanie nr 13 (z C03 L05)", action: People),

            // C04
            new CommandAction(key: "Knowledge", actionName: "Zadanie nr 14 (z C04 L01)", action: Knowledge),
            new CommandAction(key: "Tools", actionName: "Zadanie nr 15 (z C04 L02)", action: Tools),
            new CommandAction(key: "Gnome", actionName: "Zadanie nr 16 (z C04 L03)", action: Gnome),
            new CommandAction(key: "Ownapi", actionName: "Zadanie nr 17 (z C04 L04)", action: Ownapi),
            new CommandAction(key: "Ownapipro", actionName: "Zadanie nr 18 (z C04 L05)", action: Ownapipro),

            // C05
            new CommandAction(key: "Meme", actionName: "Zadanie nr 19 (z C05 L01)", action: Meme),
            new CommandAction(key: "Optimaldb", actionName: "Zadanie nr 21 (z C05 L02)", action: Optimaldb),
            new CommandAction(key: "Google", actionName: "Zadanie nr 22 (z C05 L03)", action: Google),
            new CommandAction(key: "Md2html", actionName: "Zadanie nr 23 (z C05 L04)", action: Md2html),
        };

        public static async Task<int> End()
        {
            Environment.Exit(0);
            return await Task.FromResult(0);    // A Task<int> is expected here therefore we use Task.FromResult to wrap our integer result in a Task
        }

    }
}