using System.Net;
using Microsoft.Extensions.Logging;
using Models;
using static Models.Endpoint;

public interface IEndpoint
{
    string Ip { get; set; }
    string Mac { get; set; }
    int Status { get; set; }
    long Bytes_out { get; set; }
    long Bytes_in { get; set; }
    long RTT { get; set; }

    IList<Endpoint> EndpointToTabell();
    IList <EndpointItem> LoadJson(); 
    
    void Run();
}