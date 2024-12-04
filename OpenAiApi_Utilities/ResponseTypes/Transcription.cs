using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpenAiApi_Utilities.ResponseTypes
{

    public class Transcription
    {
        public string Task { get; set; }
        public string Language { get; set; }
        public double Duration { get; set; }
        public string Text { get; set; }
        public List<Segment> Segments { get; set; }
    }

    public class Segment
    {
        public int Id { get; set; }
        public int Seek { get; set; }
        public double Start { get; set; }
        public double End { get; set; }
        public string Text { get; set; }
        public List<int> Tokens { get; set; }
        public double Temperature { get; set; }
        public double AvgLogprob { get; set; }
        public double CompressionRatio { get; set; }
        public double NoSpeechProb { get; set; }
    }
}