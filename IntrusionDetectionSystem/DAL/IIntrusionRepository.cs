using Models;

namespace IntrusionDetectionSystem.DAL
{
    public interface IIntrusionRepository
    {
        Task<List<Endpoints>> GetAllEndpoints();
        Task<Endpoints> GetEndpointById(int id);
        Task<bool> CreateNewEndpointInDb(string ip, bool isWhitelist, string mac_address);
        Task<Endpoints> GetEndpointByIP(string ip);
        Task<List<long>> GetParamValuesByTime(string ip, string parameter, long timestamp);
        Task<int> AddNewConnectionToEndpoint(Connections con, Endpoints end);
    }
}