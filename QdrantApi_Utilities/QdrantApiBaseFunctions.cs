using Newtonsoft.Json;
using QdrantApi_Utilities.ResponseTypes;
using QdrantApi_Utilities.TypesClass;
using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace QdrantApi_Utilities
{
    public static class QdrantApiBaseFunctions
    {
        public static void AddNewEntryToThePoints(ref PointsClass[] existingTable, int id, double[] vector, object payload)
        {
            var newEntry = new PointsClass
            {
                id = id,
                vector = vector,
                payload = payload,
            };
            existingTable = existingTable.Concat(new[] { newEntry }).ToArray();
        }

        public static async Task<string> GetResponse(HttpMethod methodType, string requestUri, HttpContent content = null)
        {
            using (var client = QdrantClient.CreateClient())
            {
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(method: methodType, requestUri: requestUri);
                    if (content != null)
                    {
                        request.Content = content;
                    }
                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    throw new MyException($"{response.StatusCode}");
                }
                catch (HttpRequestException ex)
                {
                    throw new MyException(ex.Message);
                }
            }
        }

    }
}