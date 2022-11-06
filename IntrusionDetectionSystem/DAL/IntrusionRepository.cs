
using Microsoft.Extensions.Logging;
using Models;
using Microsoft.EntityFrameworkCore;


namespace IntrusionDetectionSystem.DAL
{
    public class IntrusionRepository : IIntrusionRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<Endpoint> _log;


        public IntrusionRepository(AppDbContext db, ILogger<Endpoint> log)
        {
            _db = db;
            _log = log;
        }
        public async Task<bool> CreateNewEndpoint(string ip, int status, float bytes_in, float bytes_out, TimeSpan rtt)
        {
            try
            {
                Endpoint endpoint = new Endpoint();
                endpoint.Ip = ip;
                endpoint.Status = status;
                endpoint.Bytes_in = bytes_in;
                endpoint.Bytes_out = bytes_out;
                endpoint.RTT = rtt;

                _db.Endpoints.Add(endpoint);
                await _db.SaveChangesAsync();
                return true; 
            }
            catch (Exception e)
            {
                //Log error here!
                return false; 
            }



        }

        public async Task<List<Endpoint>> GetAllEndpoints()
        {
            try
            {
                List<Endpoint> allEndpoints = await _db.Endpoints.ToListAsync();
                return allEndpoints;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Endpoint> GetEndpointById(int id)
        {
            Endpoint endpoint = await _db.Endpoints.FindAsync(id);
            return endpoint;
        }



    }
}