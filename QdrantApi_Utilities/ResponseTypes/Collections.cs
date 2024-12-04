using Newtonsoft.Json;
using System.Collections.Generic;

namespace QdrantApi_Utilities.ResponseTypes
{
    public class ResultBase
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("time")]
        public double Time { get; set; }
    }

    public class GetCollections : ResultBase
    {
        [JsonProperty("result")]
        public ResultObj Result { get; set; }

        public class ResultObj
        {
            [JsonProperty("collections")]
            public List<Collection> Collections { get; set; }

            public class Collection
            {
                [JsonProperty("name")]
                public string Name { get; set; }
            }
        }
    }

    public class CreateCollectionResult : ResultBase
    {
        [JsonProperty("result")]
        public bool Result { get; set; }
    }

    public class CreatePointsResult : ResultBase
    {
        [JsonProperty("result")]
        public ResultObj Result { get; set; }

        public class ResultObj
        {
            [JsonProperty("operation_id")]
            public int OperationId { get; set; }

            public class Collection
            {
                [JsonProperty("status")]
                public string Status { get; set; }
            }
        }
    }


}
