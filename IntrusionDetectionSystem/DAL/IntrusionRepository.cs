
using System.Numerics;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<bool> CreateNewEndpointInDb(string ip, bool isWhitelist, string mac_address, int conn_id)
        {
            try
            {
                Endpoints new_endpoint_toDb = new Endpoints();
                new_endpoint_toDb.ip_address = ip;
                new_endpoint_toDb.mac_address = mac_address;
                new_endpoint_toDb.whitelist = isWhitelist;

                _db.Endpoints.Add(new_endpoint_toDb);
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
                List<Endpoints> allEndpoints = await _db.Endpoints.ToListAsync();
                return allEndpoints;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Endpoints> GetEndpointById(int id)
        {
            Endpoints endpoint = await _db.Endpoints.FindAsync(id);
            return endpoint;
        }


        public async Task<Endpoints> GetEndpointByIP(string ip)
        {
            try
            {
                Endpoints endpoint = await _db.Endpoints.FirstOrDefaultAsync(endp => endp.ip_address == ip);
                return endpoint;
            }
            catch
            {
                return null; 
            }

        }

        public async Task<Endpoints> GetConnection_ByEndpointIP(string ip)
        {
            Endpoints endpoint_byIP = await GetEndpointByIP(ip); 
            return endpoint_byIP; 
        }

        public async Task<int> AddNewConnectionToEndpoint(Connections con, Endpoints end) 
        {
            Endpoints endpoint = await GetEndpointByIP(end.ip_address); 
            //endpoint.connections.Add(con);
            await _db.SaveChangesAsync();  
            int id = con.conn_id; 
            return id; 
        }
    
        /**
         * Method to query and compute average values from DB
         * Returns an array, [avgBytesOut, avgBytesIn, avgRTT] for the given timewindow
         */
        public async Task <Array> GetAverageByIP(string ip, long startTime, long endTime)
        {
            var BytesOut = 
                from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > startTime && con.timestamp < endTime)
                                select con.bytes_out;
            
            var BytesIn = 
                from con in _db.Connections
                where (con.ip_address == ip && con.timestamp > startTime && con.timestamp < endTime)
                select con.bytes_in;
            
            
            var Rtt = 
                from con in _db.Connections
                where (con.ip_address == ip && con.timestamp > startTime && con.timestamp < endTime)
                select con.rtt;

            //Compute averages
            double avgBytesOut = BytesOut.Average();
            double avgBytesIn = BytesIn.Average();
            double avgRtt = Rtt.Average();
            
            double[] AvgByTime =
            {
                avgBytesOut,
                avgBytesIn,
                avgRtt
            };
           
            return AvgByTime;

             
            /* // retrieve all values simultaneously - doesnt work to compute average
            var hour = 
                from con in _db.Connections
                where con.ip_address == ip && con.timestamp == 1203213
                select new
                {
                    BytesOut = con.bytes_out, 
                    BytesIn = con.bytes_in, 
                    Rtt = con.rtt
                };
            */
        }
        

    }
}