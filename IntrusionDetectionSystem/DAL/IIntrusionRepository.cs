using Models;

namespace IntrusionDetectionSystem.DAL
{
    public interface IIntrusionRepository
    {
        Task<List<Endpoints>> GetAllEndpoints();
        Task<Endpoints> GetEndpointById(int id); 
        Task<bool> CreateNewEndpointInDb(string ip, bool isWhitelist, string mac_address, int conn_id); 
        Task <Endpoints> GetEndpointByIP(string ip);
        Task <Array> GetAverageByIP(string ip, long startTime, long endTime);
        Task <int> AddNewConnectionToEndpoint(Connections con, Endpoints end);
    }   
}