using Models;

namespace IntrusionDetectionSystem.DAL
{
    public interface IIntrusionRepository
    {
        Task<List<Endpoints>> GetAllEndpoints();
        Task<Endpoints> GetEndpointById(int id); 
        Task<bool> CreateNewEndpointInDb(string ip, bool isWhitelist, string mac_address, int conn_id); 
        Task <Endpoints> GetEndpointByIP(string ip);
        Task<List<long>> GetBytesOutByIp(string ip);
        Task<List<long>> GetBytesInByIp(string ip);
        Task<List<long>> GetRttByIp(string ip);
        Task<List<int>> GetCountByIp(string ip);
        Task <int> AddNewConnectionToEndpoint(Connections con, Endpoints end);
    }   
}