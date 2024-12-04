using Newtonsoft.Json;

namespace AiDevsApi_Utilities.ResponseTypes
{

    public class AnswerResponse : BaseResponse
    {
        [JsonProperty(PropertyName = "note")]
        public string Note { get; set; }

        [JsonProperty(PropertyName = "reply")]
        public string Reply { get; set; }
    }

}