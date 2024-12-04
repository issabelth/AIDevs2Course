using OpenAiApi_Utilities.Data.Enums;
using OpenAiApi_Utilities.ResponseTypes;
using System.Collections.Generic;

namespace OpenAiApi_Utilities.Data.Dictionaries
{
    public partial class OpenAiDictionaries
    {

        public static Dictionary<GptModel, string> GptModels = new Dictionary<GptModel, string>
        {
            { GptModel.gpt35turbo, "gpt-3.5-turbo" },
            { GptModel.gpt40613, "gpt-4-0613" },
            { GptModel.textEmbeddinAda002, "text-embedding-ada-002" },
            { GptModel.whisper1, "whisper-1" },
            { GptModel.textembedding3small, "text-embedding-3-small" },
            { GptModel.gpt4turbo, "gpt-4-turbo" },
        };
    }
}