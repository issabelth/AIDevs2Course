using Newtonsoft.Json;
using OpenAiApi_Utilities;
using OpenAiApi_Utilities.Data.Enums;
using OpenAiApi_Utilities.ResponseTypes;
using QdrantApi_Utilities;
using QdrantApi_Utilities.TypesClass;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace AIDevs_Course.ConsoleCommands
{

    /// <summary>
    /// Commands_Qdrant - akcje związane z Qdrant
    /// </summary>
    public partial class CommandsList
    {

        private static async Task<int> GetCollections()
        {
            return await QdrantApiFunctions.WriteCollectionsNames();
        }

        private static async Task<int> GetCollection()
        {
            Console.WriteLine("Podaj nazwę szukanej kolekcji:");
            string collectionName = Console.ReadLine();
            return await QdrantApiFunctions.GetCollection(collectionName);
        }

        private static async Task<int> CreateCollection()
        {
            Console.WriteLine("Podaj nazwę dla nowej kolekcji:");
            string collectionName = Console.ReadLine();
            return await QdrantApiFunctions.CreateCollection(collectionName, vectorSize: 4, distance: QdrantApiFunctions.DistanceEnum.Dot);
        }

        private static async Task<int> CreatePoints()
        {
            Console.WriteLine("Podaj nazwę dla kolekcji:");
            string collectionName = Console.ReadLine();

            var pointsTable = new PointsClass[0];
            int id = 1;

            var objects = new[]
            {
                new { title = "tytuł 1" , link = "link 1"},
                new { title = "tytuł 2" , link = "link 2"},
                new { title = "tytuł 3" , link = "link 3"},
            };

            foreach (var obj in objects)
            {
                //var embeddedTitle = OpenAiApiFunctions.Embeddings_ReturnOnlyEmbedding(input: obj.title, gptModel: GptModel.textembedding3small).ToArray();

                QdrantApiBaseFunctions.AddNewEntryToThePoints(
                    existingTable: ref pointsTable,
                    id: id++,
                    vector: CreateRandomTable(),
                    payload: obj);
            }

            return await QdrantApiFunctions.AddVectors(collectionName, pointsTable);
        }
        public static double[] CreateRandomTable()
        {
            Random rand = new Random();
            double[] table = new double[4];
            for (int i = 0; i < 4; i++)
            {
                table[i] = rand.NextDouble();
            }
            return table;
        }

    }
}