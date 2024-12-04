namespace OpenAiApi_Utilities.Data
{
    public class GptConversationEntry
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class GptConversationEntry_Vision
    {
        public string role { get; set; }
        public object[] content { get; set; }
    }
}