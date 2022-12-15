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
    IList<EndpointItem> list;

    // Tabell av endpoints 
    IList<Endpoint> EndPoints = new List<Endpoint>();

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
    
    public string Mac { get; set; }
    public int Status { get; set; }
    public long Bytes_out { get; set; }
    public long Bytes_in { get; set; }
    public long RTT { get; set; }



    public class Data
    {   [JsonPropertyName("Data")]
        public List <EndpointItem> listOfIps {get; set; }
    
    }

    
    public IList <EndpointItem> LoadJson()
    {
        string path = "DAL/whitelist.json";
        using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            try
            {
                var options = new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true //Allow trailing commas 
                }; 
                Data data = JsonSerializer.Deserialize<Data>(f,options)!;
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
                list = new List<EndpointItem> { item };
                return list; 
            }

           

        }
    }


    public  IList<Endpoint> EndpointToTabell()
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


