using Newtonsoft.Json;
using QdrantApi_Utilities.ResponseTypes;
using QdrantApi_Utilities.TypesClass;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace QdrantApi_Utilities
{
    public static class QdrantApiFunctions
    {

        public static async Task<int> WriteCollectionsNames()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Próba wypisywania kolekcji z bazy wektorowej...");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                string content = await QdrantApiBaseFunctions.GetResponse(
                    methodType: HttpMethod.Get,
                    requestUri: "collections");

                var collectionsObject = JsonConvert.DeserializeObject<GetCollections>(content);

                foreach (var collection in collectionsObject.Result.Collections)
                {
                    Console.WriteLine(collection.Name);
                }
            }
            catch (MyException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    $"QdrantApiFunctions.WriteCollectionsNames():{Environment.NewLine}" +
                    $"{ex.Message}{Environment.NewLine}{ex.InnerException}{Environment.NewLine}" +
                    $"Sprawdź, czy na pewno uruchomiłeś Qdrant.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            return await Task.FromResult(0);
        }

        public static async Task<int> GetCollection(string collectionName)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Próba pobierania kolekcji z bazy wektorowej...");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                string content = await QdrantApiBaseFunctions.GetResponse(
                    methodType: HttpMethod.Get,
                    requestUri: $"collections/{collectionName}");

                var collection = JsonConvert.DeserializeObject<ResultBase>(content);
                Console.WriteLine(collection.Status);
            }
            catch (MyException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    $"QdrantApiFunctions.GetCollection():{Environment.NewLine}" +
                    $"{ex.Message}{Environment.NewLine}{ex.InnerException}{Environment.NewLine}" +
                    $"Sprawdź, czy na pewno uruchomiłeś Qdrant.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            return await Task.FromResult(0);
        }

        public enum DistanceEnum
        {
            Dot = 1,
            Cosine = 2
        };

        public static async Task<dynamic> CreateCollection(string collectionName, int vectorSize, DistanceEnum distance)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Próba tworzenia kolekcji w bazie wektorowej...");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    vectors = new
                    {
                        size = vectorSize,
                        distance = distance.ToString()
                    }
                });

                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    string findColl = await QdrantApiBaseFunctions.GetResponse(
                        methodType: HttpMethod.Get,
                        requestUri: $"collections/{collectionName}");
                }
                catch (Exception ex)
                {
                    if (ex.Message != "NotFound")
                    {
                        throw new MyException(ex.Message);
                    }
                    // jak not found to idę dalej i tworzę
                }

                string content = await QdrantApiBaseFunctions.GetResponse(
                    methodType: HttpMethod.Put,
                    requestUri: $"collections/{collectionName}",
                    content: jsonContent);

                var result = JsonConvert.DeserializeObject<dynamic>(content);
                Console.WriteLine($"{result.status}");
                return Task.FromResult(result);
            }
            catch (MyException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    $"QdrantApiFunctions.CreateCollection():{Environment.NewLine}" +
                    $"{ex.Message}{Environment.NewLine}{ex.InnerException}{Environment.NewLine}" +
                    $"Sprawdź, czy na pewno uruchomiłeś Qdrant.");
                Console.ForegroundColor = ConsoleColor.White;
                return await Task.FromResult(-1);
            }
        }

        public static async Task<int> AddVectors(string collectionName, PointsClass[] pointsTable)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Próba dodania wektorów do bazy...");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    points = pointsTable
                });

                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

                string content = await QdrantApiBaseFunctions.GetResponse(
                    methodType: HttpMethod.Put,
                    requestUri: $"collections/{collectionName}/points",
                    content: jsonContent);

                var result = JsonConvert.DeserializeObject<CreatePointsResult>(content);
                Console.WriteLine($"{result.Status}");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Dodano wektory do bazy");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (MyException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    $"QdrantApiFunctions.AddVectors():{Environment.NewLine}" +
                    $"{ex.Message}{Environment.NewLine}{ex.InnerException}{Environment.NewLine}" +
                    $"Sprawdź, czy na pewno uruchomiłeś Qdrant.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            return await Task.FromResult(0);
        }

        public static async Task<dynamic> Search(string collectionName, string json, string methodName = "search")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Próba wyszukiwania w bazie wektorowej...");
            Console.ForegroundColor = ConsoleColor.White;

            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            string content = await QdrantApiBaseFunctions.GetResponse(
                methodType: HttpMethod.Post,
                requestUri: $"collections/{collectionName}/points/{methodName}",
                content: jsonContent);

            var result = JsonConvert.DeserializeObject<dynamic>(content);
            Console.WriteLine($"{result.Status}");
            return await Task.FromResult(result);
        }


    }
}