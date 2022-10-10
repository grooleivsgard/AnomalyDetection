using Microsoft.EntityFrameworkCore;
using Models;

namespace IntrusionDetectionSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Connection> Connections { get; set; }
    }
}