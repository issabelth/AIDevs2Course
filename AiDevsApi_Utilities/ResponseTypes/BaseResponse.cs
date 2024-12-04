using Newtonsoft.Json;

namespace AiDevsApi_Utilities.ResponseTypes
{

    public class BaseResponse
    {
        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }
    }

}