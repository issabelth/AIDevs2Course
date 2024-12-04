using Newtonsoft.Json;
using System;

namespace AiDevsApi_Utilities.ResponseTypes
{
    public class BaseAiDevsClass
    {
        [JsonIgnore]
        public string DataToEmbed { get; set; }
    }

    public class UnknownNewsObj : BaseAiDevsClass
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Info { get; set; }
        public DateTime Date { get; set; }

        public UnknownNewsObj()
        {
            DataToEmbed = Title;
        }
    }

    public class PeopleObj : BaseAiDevsClass
    {
        [JsonProperty("imie")]
        public string Imie { get; set; }

        [JsonProperty("nazwisko")]
        public string Nazwisko { get; set; }

        [JsonProperty("wiek")]
        public string Wiek { get; set; }

        [JsonProperty("o_mnie")]
        public string O_mnie { get; set; }

        [JsonProperty("ulubiona_postac_z_kapitana_bomby")]
        public string Bomba { get; set; }

        [JsonProperty("ulubiony_serial")]
        public string Serial { get; set; }

        [JsonProperty("ulubiony_film")]
        public string Film { get; set; }

        [JsonProperty("ulubiony_kolor")]
        public string Kolor { get; set; }

        public PeopleObj()
        {
            DataToEmbed = O_mnie;
        }
    }
}