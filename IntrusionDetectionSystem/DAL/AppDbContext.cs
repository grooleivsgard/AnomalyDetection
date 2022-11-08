using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntrusionDetectionSystem.DAL;
using Microsoft.EntityFrameworkCore;
using Models;

namespace IntrusionDetectionSystem.DAL
{

    public class Connections 
    {
        [Key]
        public int conn_id {get; set;}
        public long bytes_in {get; set;}

        public long bytes_out {get; set;}

        // rtt => Round trip time 
        public long rtt {get; set; }

        public long timestamp {get; set;}
        
        [ForeignKey("Endpoints")]
        public string ip_address {get; set;}
        
       
    }

    public class Endpoints 
    {
        [Key]
        public string ip_address {get; set;}
        public bool whitelist {get; set;}
        public string mac_address {get; set;}
        [Key]
        public int latest_conn_id {get; set;} 
        public Array ConnectionIDs {get; set;}
        public virtual List<Connections> connections {get; set;}
        
    }

     


    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)

        {
            Database.EnsureDeleted(); 
            Database.EnsureCreated();
        }

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies(); 
        }

        public DbSet<Endpoints> Endpoints { get; set; }
        public DbSet<Connections> Connections {get; set;}
    }
}