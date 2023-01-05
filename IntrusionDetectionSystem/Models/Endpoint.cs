using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using IntrusionDetectionSystem;
using Microsoft.Extensions.Logging;

namespace Models;



public  class Endpoint : IEndpoint

{
    private readonly ILogger<Startup>? _log;
    IList<EndpointItem>? list;

    // Tabell av endpoints 
    IList<Endpoint> EndPoints = new List<Endpoint>();

    public Endpoint(ILogger<Startup> log)
    {
        _log = log; 
}
    
    public Endpoint( )
    {
        isStempled = (Bytes_out != -1 && Bytes_in != -1); 
    }

    [Key]
    public int conn_Id {get; set; }
    public string? Ip { get; set; }
    public string? Mac { get; set; }
    public int State { get; set; }
    //Set the default value to -1, means this value was never modified 
    public long Bytes_out { get; set; } = -1; 
    public long Bytes_in { get; set; }= -1; 
    public long RTT { get; set; }
    //Set the default value to false
    public bool isAnomolous {get; set;} = false; 
    public string? anomalityReport {get; set;}
    public bool isStempled {get; set;}

    public class Data
    {   [JsonPropertyName("Data")]
        public List <EndpointItem> listOfIps {get; set; }
    
    }

    
    public IList <EndpointItem> LoadJson()
    {
        string? path = "DAL/whitelist.json";
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
                if (data!.listOfIps is null) _log!.LogWarning("Error inside LoadJson Function: data.listOfIps is null"); 
                if (data!.listOfIps is null) Console.WriteLine("Error inside LoadJson Function: data.listOfIps is null"); 
                else this.list = data.listOfIps; 
                return list!; 

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
        
        foreach (EndpointItem item in list!)
        {
            Endpoint e = new Endpoint();
            e.Ip = item.IP;
            e.Mac =  item.MAC; 
            EndPoints.Add(e);
            Console.WriteLine(e.Ip +  " & Mac adrress: " + e.Mac);
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
        public string MAC {get; set; }
    }

    public interface IEndpointItem 
    {
        string IP { get; set; }
        string Id { get; set; }
        string MAC {get; set; }
    }


