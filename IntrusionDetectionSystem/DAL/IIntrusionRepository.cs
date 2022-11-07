using Models;

namespace IntrusionDetectionSystem.DAL
{
    public interface IIntrusionRepository
    {
        Task<List<Endpoints>> GetAllEndpoints();
        Task<Endpoints> GetEndpointById(int id); 
        Task<bool> CreateNewEndpointInDb(string ip, bool isWhitelist, string mac_address, int conn_id); 
        Task <Endpoints> GetEndpointByIP(string ip);
        Task AddNewConnectionToEndpoint(Connections con, Endpoints end);
    }   
}