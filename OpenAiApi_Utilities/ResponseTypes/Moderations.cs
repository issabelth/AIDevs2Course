using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpenAiApi_Utilities.ResponseTypes
{
    public class Moderations
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("results")]
        public Result[] Results { get; set; }
    }

    public class Result
    {
        [JsonProperty("flagged")]
        public bool Flagged { get; set; }

        [JsonProperty("categories")]
        public Dictionary<string, bool> Categories { get; set; }

        [JsonProperty("category_scores")]
        public Dictionary<string, double> CategoryScores { get; set; }
    }
}
