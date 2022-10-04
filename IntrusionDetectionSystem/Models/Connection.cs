using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Connection
    {
        [Key]
        public int Id {get; set;}
        public string Name { get; set; }
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string DestinationPort { get; set; }
        public string FirstTimeSeenDate { get; set; }
         public string Instance {get; set;}
        public string Job {get; set;}

        public string toString()
        {
            return "Connection going from: " + this.SourceAddress
            + " to: " + this.DestinationAddress + " port: "
            + this.DestinationPort + "// FirstTimeSeen at: " + FirstTimeSeenDate
            + " " + this.Instance + " job: " + this.Job
            ;
        }
    }

}