
using System.Collections;
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
        
        private readonly long _startTime = 32489; // compute
        private readonly long _endTime = 32489;
        
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


        public async Task<List<long>> GetBytesOutByIp(string ip)
        {
            try
            {

                long SumHourly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.bytes_out).Sum();

                long SumDaily =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.bytes_out).Sum();

                long SumWeekly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.bytes_out).Sum();

                List<long> SumBytesOut = new List<long>();

                SumBytesOut.Add(SumHourly);
                SumBytesOut.Add(SumDaily);
                SumBytesOut.Add(SumWeekly);

                return SumBytesOut;


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
            catch
            {
                return null;
            }
        }

        public async Task<List<long>> GetBytesInByIp(string ip)
        {
            try
            {
                
                long SumHourly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.bytes_in).Sum();

                long SumDaily =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.bytes_in).Sum();

                long SumWeekly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.bytes_in).Sum();


                List<long> SumBytesIn = new List<long>();

                SumBytesIn.Add(SumHourly);
                SumBytesIn.Add(SumDaily);
                SumBytesIn.Add(SumWeekly);

                return SumBytesIn;
            }
            catch
            {
                return null;
            }
            
        }

        public async Task<List<long>> GetRttByIp(string ip)
        {
            
            try
            {
                long SumHourly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Sum();

                long SumDaily =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Sum();

                long SumWeekly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Sum();


                List<long> SumRtt = new List<long>();

                SumRtt.Add(SumHourly);
                SumRtt.Add(SumDaily);
                SumRtt.Add(SumWeekly);

                return SumRtt;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<int>> GetCountByIp(string ip)
        {
            try
            {
                int SumHourly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Count();

                int SumDaily =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Count();

                int SumWeekly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Count();


                List<int> Count = new List<int>();

                Count.Add(SumHourly);
                Count.Add(SumDaily);
                Count.Add(SumWeekly);

                return Count;
            }
            catch
            {
                return null;
            }
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

