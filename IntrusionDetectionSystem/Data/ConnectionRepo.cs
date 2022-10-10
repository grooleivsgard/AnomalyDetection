using Models;

namespace IntrusionDetectionSystem.Data 
{
    public class ConnectionRepo: IConnectionRepo
    {
        private readonly AppDbContext _dbContext;

        public ConnectionRepo(AppDbContext dbContext)
        {
            _dbContext = dbContext; 
        }
        public void CreateAConnection()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Connection> GetAllConnections()
        {
            return _dbContext.Connections.ToList(); 
        }

        public Connection GetConnectionById(int id)
        {
            return _dbContext.Connections.FirstOrDefault(Connection => Connection.Id == id); 
        }
        public bool SaveChanges()
        {
            // Return a value greater than one if the Changes were saved in Database
            return (_dbContext.SaveChanges() >= 0);  
        }
        
       
    }
}   