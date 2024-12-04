using OpenAiApi_Utilities.Data.Enums;
using System.Collections.Generic;

namespace OpenAiApi_Utilities.Data.Dictionaries
{
    public partial class OpenAiDictionaries
    {

        public static Dictionary<ModerationsModel, string> ModerationsModels = new Dictionary<ModerationsModel, string>
        {
            { ModerationsModel.latest, "text-moderation-latest" },
        };
    }
}