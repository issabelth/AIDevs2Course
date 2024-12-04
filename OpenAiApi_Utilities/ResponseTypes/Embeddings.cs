using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpenAiApi_Utilities.ResponseTypes
{
    public class Embeddings
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("data")]
        public List<EmbeddingData> Data { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("usage")]
        public BasicUsage Usage { get; set; }
    }

    public class EmbeddingData
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("embedding")]
        public List<double> Embedding { get; set; }
    }

    public class BasicUsage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }
}