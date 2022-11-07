using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using IntrusionDetectionSystem;
using Microsoft.Extensions.Logging;

namespace Models;



public  class Endpoint : IEndpoint

{
    private readonly ILogger<Startup> _log;
    IList<IEndpointItem> list;

    // Tabell av endpoints 
    IList<IEndpoint> EndPoints = new List<IEndpoint>();

    public Endpoint(ILogger<Startup> log)
    {
        _log = log; 
}
    
    public Endpoint( )
    {
        
    }

    [Key]
    public int conn_Id {get; set; }
    public string Ip { get; set; }
    public int Status { get; set; }
    public float Bytes_out { get; set; }
    public float Bytes_in { get; set; }

    // RTT : Round-trip-time
    public TimeSpan? RTT { get; set; }


    public class Data
    {   [JsonPropertyName("Data")]
        public List <IEndpointItem> listOfIps {get; set; }
    
    }

    
    public IList <IEndpointItem> LoadJson()
    {
        using (FileStream f = new FileStream("../IntrusionDetectionSystem/DAL/WhiteList.json", FileMode.Open, FileAccess.Read))
        {
            try
            {
                Data data = JsonSerializer.Deserialize<Data>(f)!;
                //if (data is null ) _log.LogWarning("Error inside LoadJson Function: data is null");      
                if (data is null ) Console.WriteLine("Error inside LoadJson Function: data is null");      
                if (data!.listOfIps is null) _log.LogWarning("Error inside LoadJson Function: data.listOfIps is null"); 
                if (data!.listOfIps is null) Console.WriteLine("Error inside LoadJson Function: data.listOfIps is null"); 
                else this.list = data.listOfIps; 
                return list; 

            }

            catch (Exception e)
            {
                Console.WriteLine("Error Reading/Deserializing  WhiteList: " + e.Message);
                //_log.LogError("Error Reading/Deserializing  WhiteList: " + e.Message);
                // In case of an Exception return 
                EndpointItem item = new EndpointItem();
                item.Id = "-1";
                item.IP = "0.0.0.0";
                list = new List<IEndpointItem> { item };
                return list; 
            }

           

        }
    }


    public  IList<IEndpoint> EndpointToTabell()
    {
        
        foreach (EndpointItem item in list)
        {
            Endpoint e = new Endpoint();
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

public class EndpointItem: IEndpointItem
    {
        public string IP { get; set; }
        public string Id { get; set; }
    }

    public interface IEndpointItem 
    {
        string IP { get; set; }
        string Id { get; set; }
    }


