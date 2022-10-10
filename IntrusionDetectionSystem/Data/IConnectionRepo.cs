using Models;

namespace IntrusionDetectionSystem.Data 
{
    public interface IConnectionRepo 
    {
        bool SaveChanges(); 
        IEnumerable<Connection> GetAllConnections(); 
        void CreateAConnection(); 
    }
}