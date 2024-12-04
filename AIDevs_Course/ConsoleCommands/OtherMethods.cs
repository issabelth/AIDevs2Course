using AiDevsApi_Utilities.ResponseTypes;
using Newtonsoft.Json;
using OpenAiApi_Utilities;
using OpenAiApi_Utilities.Data.Enums;
using QdrantApi_Utilities;
using QdrantApi_Utilities.TypesClass;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AIDevs_Course.ConsoleCommands
{

    /// <summary>
    /// OtherMethods
    /// </summary>
    public partial class CommandsList
    {

        public static async Task ImportJsonToQdrant<T>(string jsonUrl, string collName) where T : BaseAiDevsClass
        {
            Console.WriteLine("Czy zaimportować dane z JSONa do bazy wektorowej? (T/N)");
            var answer = Console.ReadKey();

            if (answer.Key != ConsoleKey.T)
            {
                return;
            }

            #region import listy do bazy wektorowej

            var clientForList = new HttpClient();
            var json = await clientForList.GetStringAsync(jsonUrl);
            T[] objTable = JsonConvert.DeserializeObject<T[]>(json);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Pobrano JSONa");
            Console.ForegroundColor = ConsoleColor.White;

            await QdrantApiFunctions.CreateCollection(collectionName: collName, vectorSize: 1536, distance: QdrantApiFunctions.DistanceEnum.Cosine);

            var pointsTable = new PointsClass[0];
            int id = 1;
            int objCount = objTable.Count();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Tworzenie listy wektorów...");
            Console.ForegroundColor = ConsoleColor.White;

            foreach (var obj in objTable)
            {
                double[] embedded = null;

                if (obj is PeopleObj pplObj)
                {
                    embedded = OpenAiApiFunctions.Embeddings_ReturnOnlyEmbedding(input: pplObj.O_mnie, gptModel: GptModel.textembedding3small).ToArray();

                    var optimizedObj = new
                    {
                        imie = pplObj.Imie,
                        nazwisko = pplObj.Nazwisko,
                        informacje = $"{pplObj.O_mnie}. Ulubiony kolor: {pplObj.Kolor}. Ulubiony film: {pplObj.Film}. " +
                            $"Ulubiony serial: {pplObj.Serial}. Ulubiona postać z kapitana bomby: {pplObj.Bomba}. Wiek: {pplObj.Wiek}",
                    };

                    QdrantApiBaseFunctions.AddNewEntryToThePoints(
                        existingTable: ref pointsTable,
                        id: id++,
                        vector: embedded,
                        payload: optimizedObj);
                }
                else if (obj is UnknownNewsObj unknObj)
                {
                    embedded = OpenAiApiFunctions.Embeddings_ReturnOnlyEmbedding(input: unknObj.Title, gptModel: GptModel.textembedding3small).ToArray();

                    QdrantApiBaseFunctions.AddNewEntryToThePoints(
                        existingTable: ref pointsTable,
                        id: id++,
                        vector: embedded,
                        payload: obj);
                }
                else
                {
                    throw new NotImplementedException("Nie zdefiniowano klasy w ImportJsonToQdrant()");
                }

                if (id % 100 == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{id}/{objCount}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Tworzenie listy wektorów ok");
            Console.ForegroundColor = ConsoleColor.White;

            await QdrantApiFunctions.AddVectors(collectionName: collName, pointsTable: pointsTable);

            #endregion import listy do bazy wektorowej
        }

    }
}