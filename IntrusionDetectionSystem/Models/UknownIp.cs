using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class UnknownIp
    {
        [Key]
        public int Id {get; set; }
        public string Ip { get; set; }
        public float Bytes { get; set; }
        public string timestamp { get; set; }
    }
}