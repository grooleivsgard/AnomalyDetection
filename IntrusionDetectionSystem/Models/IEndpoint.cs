using System.Net;
using Microsoft.Extensions.Logging;
using Models;
using static Models.Endpoint;

public interface IEndpoint
{
    string Ip { get; set; }
    int Status { get; set; }
    float Bytes_out { get; set; }
    float Bytes_in { get; set; }
    TimeSpan? RTT { get; set; }

    IList<IEndpoint> EndpointToTabell();
    IList <IEndpointItem> LoadJson(); 
    
    void Run();
}