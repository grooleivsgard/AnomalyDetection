namespace Models
{
    public class Connection
    {
        public string Name{ get; set; }
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string DestinationPort { get; set; }
        public string FirstTimeSeenDate { get; set;}
    }
}