using Microsoft.EntityFrameworkCore;
using Models;

namespace Intrusion_Detection_System.Models
{
    public interface IEndpointDB
    {
        DbSet<Endpoint> Endpoints { get; set; }
    }
}