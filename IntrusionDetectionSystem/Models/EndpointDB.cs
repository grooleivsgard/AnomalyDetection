using Intrusion_Detection_System.Models;
using Microsoft.EntityFrameworkCore;
using Models;

namespace IntrusionDetectionSystem.Models
{


    public class EndpointDB : DbContext, IEndpointDB
    {
        public EndpointDB(DbContextOptions<EndpointDB> options) : base(options)

        {
            Database.EnsureCreated();
        }

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies(); 
        }

        public DbSet<Endpoint> Endpoints { get; set; }
        //public DbSet<UnknownIp> UnknownIps {get; set;}
    }
}