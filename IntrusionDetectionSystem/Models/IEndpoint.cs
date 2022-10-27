using System.Net;
using Models;
using static Models.Endpoint;

public interface IEndpoint
{
    string Ip { get; set; }
    int Status { get; set; }
    float Bytes_out { get; set; }
    float Bytes_in { get; set; }
    DateTime RTT { get; set; }

    List<Endpoint> EndpointToTabell();
     List <EndpointItem> LoadJson(); 
    
    void Run();
}