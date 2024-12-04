using Newtonsoft.Json;

namespace AiDevsApi_Utilities.ResponseTypes
{

    public class TokenResponse : BaseResponse
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }

}