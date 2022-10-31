using Microsoft.EntityFrameworkCore;
using Models;

namespace IntrusionDetectionSystem.Models 
{
    public class EndpointDB: DbContext 
    {
        public EndpointDB(DbContextOptions<EndpointDB> options) : base(options)

        {
           Database.EnsureCreated();  
        }

        public DbSet<Endpoint> Endpoints {get; set;}
        
    }
}