using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QdrantApi_Utilities.ResponseTypes
{
    public class SearchResultPayload
    {
        [JsonProperty("Date")]
        public DateTime Date { get; set; }

        [JsonProperty("Info")]
        public string Info { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }
    }

    public class SearchObj
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("score")]
        public float Score { get; set; }

        [JsonProperty("payload")]
        public dynamic Payload { get; set; }

        [JsonProperty("vector")]
        public object Vector { get; set; }
    }

    public class SearchResult
    {
        [JsonProperty("result")]
        public List<SearchObj> Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("time")]
        public double Time { get; set; }
    }
}