using Models;

namespace IntrusionDetectionSystem.DAL
{
    public interface IIntrusionRepository
    {
        Task<List<Endpoint>> GetAllEndpoints();
        Task<Endpoint> GetEndpointById(int id); 
         Task<bool> CreateNewEndpoint(string ip, int status, float bytes_in, float bytes_out, TimeSpan rtt); 
    }
}