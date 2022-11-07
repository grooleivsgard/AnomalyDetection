
using System.ComponentModel.DataAnnotations;
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

        public async Task<bool> CreateNewEndpoint(string ip, bool isWhitelist, string mac_address, int conn_id)
        {
            try
            {
                Endpoints new_endpoint_toDb = new Endpoints();
                new_endpoint_toDb.ip_address = ip;
                new_endpoint_toDb.mac_address = mac_address;
                new_endpoint_toDb.whitelist = isWhitelist;

                _db.endpoints.Add(new_endpoint_toDb);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                //Log error here!
                return false;
            }


        }

        public async Task<List<Endpoints>> GetAllEndpoints()
        {
            try
            {
                List<Endpoints> allEndpoints = await _db.endpoints.ToListAsync();
                return allEndpoints;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Endpoints> GetEndpointById(int id)
        {
            Endpoints endpoint = await _db.endpoints.FindAsync(id);
            return endpoint;
        }

        /*
        public async Task<List<long>> GetAvgBytesOut(string ip)
        {
            // var query = "SELECT date_trunc('hour', current_time - 1) from connections WHERE ip_address = ip";

         
             //select avg(bytes_in) from connections where ip_address = ip, group by hour
             
            Connections conn = await _db.connections.FindAsync();

            //Using Method Syntax
            var AverageBytesOut = Connection.connections()
                .Where(conn => conn.Ip == ip)
                .Average(bytes_out => conn.bytes_out);
            
            
            return _db.connections.Any(ip => ip == ip)
                .Where(window => )
            
            
          
            return conn;
        }
  */


    }
}