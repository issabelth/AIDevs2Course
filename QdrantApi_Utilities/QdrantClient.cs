using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QdrantApi_Utilities
{
    public static class QdrantClient
    {

        public static HttpClient CreateClient()
        {
            return new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:6333"),
            };
        }

    }
}
