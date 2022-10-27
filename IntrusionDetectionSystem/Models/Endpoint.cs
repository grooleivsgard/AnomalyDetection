using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Models;



public  class Endpoint : IEndpoint

{
    private readonly ILogger<Endpoint> _log;
    List<EndpointItem> list;

    // Tabell av endpoints 
    List<Endpoint> EndPoints = new List<Endpoint>();
    private readonly IMapper _mapper;
    public Endpoint(ILogger<Endpoint> log)
    {
        _log = log; 
    }
    
    public Endpoint( )
    {
        
    }

    [Key]
    public int Id {get; set; }
    public string Ip { get; set; }
    public int Status { get; set; }
    public float Bytes_out { get; set; }
    public float Bytes_in { get; set; }

    // RTT : Round-trip-time
    public DateTime RTT { get; set; }


    public class Data
    {   [JsonPropertyName("Data")]
        public List <EndpointItem> listOfIps {get; set; }
    
    }

    public class EndpointItem
    {
        public string IP { get; set; }
        public string Id { get; set; }
    }


    public List <EndpointItem> LoadJson()
    {
        using (FileStream f = new FileStream("../IntrusionDetectionSystem/Data/WhiteList.json", FileMode.Open, FileAccess.Read))
        {
            try
            {
                Data data = JsonSerializer.Deserialize<Data>(f)!;
                if (data is null ) _log.LogWarning("Error inside LoadJson Function: data is null");      
                if (data!.listOfIps is null) _log.LogWarning("Error inside LoadJson Function: data.listOfIps is null"); 
                else this.list = data.listOfIps; 
                return list; 

            }

            catch (Exception e)
            {
                _log.LogError("Error Reading/Deserializing  WhiteList: " + e.Message);
                // In case of an Exception return 
                EndpointItem item = new EndpointItem();
                item.Id = "-1";
                item.IP = "0.0.0.0";
                list = new List<EndpointItem> { item };
                return list; 
            }

           

        }
    }


    public  List<Endpoint> EndpointToTabell()
    {
        
        foreach (EndpointItem item in list)
        {
            Endpoint e = new Endpoint();
            e.Id = Int16.Parse(item.Id); 
            e.Ip = item.IP;
            EndPoints.Add(e);
            Console.WriteLine(e.Ip);
        }
        Console.WriteLine("After all we have a table with " + EndPoints.Count() + " Endpoints");
        return  EndPoints;
        

    }

    public void Run()
    {
        LoadJson();
        EndpointToTabell();
    }


}

