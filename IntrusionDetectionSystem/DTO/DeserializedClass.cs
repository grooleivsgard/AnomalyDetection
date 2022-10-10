using System.Text.Json.Serialization;
namespace DTO.IntrusionDetectionSystem
{
    public class Metric
    {
        [JsonPropertyName("__name__")]
        public string Name{ get; set; }
        [JsonPropertyName("a_srcAddr")]
        public string SourceAddress { get; set; }
        [JsonPropertyName("b_dstAddr")]
        public string DestinationAddress { get; set; }
        [JsonPropertyName("c_dstPort")]
        public string DestinationPort { get; set; }
        [JsonPropertyName("d_firstSeen")]
        public string FirstTimeSeenDate { get; set;}
        [JsonPropertyName("instance")]
        public string Instance {get; set;}
        [JsonPropertyName("job")]
        public string Job {get; set;}


    }
    public class Result
    {
        [JsonPropertyName("metric")]
        public Metric Metric { get; set; }
        [JsonPropertyName("value")]
        public List<object> Value;
    }
    public class Data
    {
        [JsonPropertyName("resultType")]
        public string ResultType { get; set; }        
        [JsonPropertyName("result")]
        public List<Result> Result { get; set; }
    }
    public class Root
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }
}