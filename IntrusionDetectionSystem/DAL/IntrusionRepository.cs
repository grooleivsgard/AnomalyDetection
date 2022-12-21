using System.Collections;
using System.Numerics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.InMemory.Design.Internal;

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

        public async Task<bool> CreateNewEndpointInDb(string ip, bool isWhitelist, string mac_address)
        {
            try
            {
                Endpoints new_endpoint_toDb = new Endpoints();
                new_endpoint_toDb.ip_address = ip;
                new_endpoint_toDb.whitelist = isWhitelist;
                new_endpoint_toDb.mac_address = mac_address;
               
                _db.Endpoints.Add(new_endpoint_toDb);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                //Log error here!
                _log.LogInformation("error Saving endpoint to DB. Full error message: " + e.Message); 
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
            if (endpoint is null)
            {
                return null!;
            }

            return endpoint;
        }


        public async Task<Endpoints> GetEndpointByIP(string ip)
        {
            try
            {
                Endpoints endpoint = await _db.Endpoints.FirstOrDefaultAsync(endp => endp.ip_address == ip) ?? 
                throw new ArgumentNullException("Endpoint with corresponding ip address could not be found in endpoints Table");
                if (endpoint is null)
                {
                    return null!;
                }

                return endpoint;
            }
            catch (Exception e)
            {
                _log.LogError("Error while trying to retrieve endpoint from database. Full error message: "+ e.Message); 
                return null!;
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
            if (endpoint.connections is null) endpoint.connections = new List<Connections>();
            endpoint.connections.Add(con);
            await _db.SaveChangesAsync();
            int id = con.conn_id;
            return id;
        }

        /**
         * Method retrieves data from DB and returns a list of all values. 
         */
/*
        public async Task<List<long>> GetEndpointValues(string ip, string parameter)
        {
            List<long> totalValues = new List<long>();
            
            //Assumes there is only 7 days of data
            if (parameter == "bytes_out")
            {
                var query = from con in _db.Connections
                    where (con.ip_address == ip && con.anomaly == false)
                    select con.bytes_out;
                totalValues = query.ToList();
                    
            } else if (parameter == "bytes_in")
            {
                var query = from con in _db.Connections
                    where (con.ip_address == ip && con.anomaly == false)
                    select con.bytes_in;
                totalValues = query.ToList();
                    
            } else if (parameter == "rtt")
            {
                var query = from con in _db.Connections
                    where (con.ip_address == ip && con.anomaly == false)
                    select con.rtt;
                totalValues = query.ToList();
            }
            else
            {
                throw new ArgumentException("Invalid parameter");
            }

            return totalValues;
        }
        }
        */
        public async Task<List<long>> GetParamValuesByTime(string ip, string parameter, long timestamp)
        {
            List<long> totalValues = new List<long>();

            if (parameter == "bytes_out")
            {
                var query =  from con in _db.Connections
                    where (con.ip_address == ip && con.timestamp >= timestamp && con.anomaly == false)
                    select con.bytes_out;
                totalValues =  query.ToList();
            }
            else if (parameter == "bytes_in")
            {
                var query = from con in _db.Connections
                    where (con.ip_address == ip && con.timestamp >= timestamp && con.anomaly == false)
                     select con.bytes_in;
                totalValues = query.ToList();
            }
            else if (parameter == "rtt")
            {
                var query = from con in _db.Connections
                    where (con.ip_address == ip && con.timestamp >= timestamp && con.anomaly == false)
                    select con.rtt;
                totalValues = query.ToList();
            }
            else
            {
                throw new ArgumentException("Invalid parameter");
            }

             return totalValues;
        }


        /**
 * Method to query and compute average values from DB
 * Returns an array, [avgBytesOut, avgBytesIn, avgRTT] for the given timewindow
 */

        /* --- Need to return list of same datatype - changed to individual methods for each parameter (bytes out etc)
        public async Task <Array> GetEndpointDataByIP(string ip, long startTime, long endTime)
        {
            var SumBytesOut = 
                (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > startTime && con.timestamp < endTime)
                                select con.bytes_out).Sum();
            
            var SumBytesIn = 
                (from con in _db.Connections
                where (con.ip_address == ip && con.timestamp > startTime && con.timestamp < endTime)
                select con.bytes_in).Sum();
            
            
            var SumRtt = 
                (from con in _db.Connections
                where (con.ip_address == ip && con.timestamp > startTime && con.timestamp < endTime)
                select con.rtt).Sum();

            //Retrieve number of elements
            int count =
                (from con in _db.Connections
                    where (con.ip_address == ip && con.timestamp > startTime && con.timestamp < endTime)
                    select con.conn_id).Count();


            ArrayList AvgByTime = new ArrayList();

            AvgByTime.Add(SumBytesOut);
            
            {
                ,
                SumBytesIn,
                SumRtt,
                count
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