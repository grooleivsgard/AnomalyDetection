namespace Model
{
    public class Metric
    {
        public string __name__ { get; set; }
        public string a_srcAddr { get; set; }
        public string b_dstAddr { get; set; }
        public string c_dstPort { get; set; }
        public string d_firstSeen { get; set;}
        public string instance {get; set;}
        public string job {get; set;}


    }
    public class Result
    {
        public Metric metric { get; set; }
        public List<object> value;
    }
    public class Data
    {
        public string resultType { get; set; }
        public List<Result> result { get; set; }
    }
    public class Root
    {
        public string status { get; set; }
        public Data data { get; set; }
    }
}