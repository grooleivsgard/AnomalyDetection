using IntrusionDetectionSystem.DAL;
using Microsoft.EntityFrameworkCore;
using Models;

namespace IntrusionDetectionSystem.DAL
{



    public class Endpoints 
     


    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)

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