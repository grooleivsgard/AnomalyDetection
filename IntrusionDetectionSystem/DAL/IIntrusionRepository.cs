using Models;

namespace IntrusionDetectionSystem.DAL
{
    public interface IIntrusionRepository
    {
        Task<List<Endpoints>> GetAllEndpoints();
        Task<Endpoints> GetEndpointById(int id); 
         Task<bool> CreateNewEndpoint(string ip, bool isWhitelist, string mac_address, int conn_id); 
    }
}