
using System.Collections;
using System.Numerics;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.InMemory.Design.Internal;

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

        public async Task<bool> CreateNewEndpointInDb(string ip, bool isWhitelist, string mac_address)
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
                Endpoints endpoint = await _db.Endpoints.FirstOrDefaultAsync(endp => endp.ip_address == ip);
                if (endpoint is null)
                {
                    return null!;
                }
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
            if (endpoint.connections is null) endpoint.connections = new List<Connections>();
            endpoint.connections.Add(con);
            await _db.SaveChangesAsync();
            int id = con.conn_id;
            return id;
        }

        /**
         * Method retrieves data from DB and returns a List of values (standard deviation)
         * for hourly, daily and weekly intervals
         */
        public async Task<Array> GetAverageByIP(string ip, long startTime, long endTime)
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
        public async Task<List<double>> GetBytesOutByIp(string ip)
        {
            try
            {
                //Retrieve all values - hourly - and make into list
                var hourlyQuery = from con in _db.Connections
                    where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                    select con.bytes_out;

                List<long> hourlyVals = hourlyQuery.ToList();
                
                //Retrieve all values - hourly - and make into list
                var dailyQuery = from con in _db.Connections
                    where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                    select con.bytes_out;

                List<long> dailyVals= dailyQuery.ToList(); //Retrieve all values - hourly - and make into list
                
                var weeklyQuery = from con in _db.Connections
                    where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                    select con.bytes_out;

                List<long> weeklyVals= weeklyQuery.ToList();
                

                List<double> SdBytesOut = new List<double>();

                SdBytesOut.Insert(0, Statistics.ComputeStandardDeviation(hourlyVals));
                SdBytesOut.Insert(1, Statistics.ComputeStandardDeviation(dailyVals));
                SdBytesOut.Insert(2, Statistics.ComputeStandardDeviation(weeklyVals));

                return SdBytesOut;


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

        public async Task<List<double>> GetBytesInByIp(string ip)
        {
            try
            {
                
                double AvgHourly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.bytes_in).Average();

                double AvgDaily =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.bytes_in).Average();

                double AvgWeekly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.bytes_in).Average();


                List<double> AvgBytesIn = new List<double>();

                AvgBytesIn.Insert(0, AvgHourly);
                AvgBytesIn.Insert(1, AvgDaily);
                AvgBytesIn.Insert(2, AvgWeekly);

                return AvgBytesIn;
            }
            catch
            {
                return null;
            }
            
        }

        public async Task<List<double>> GetRttByIp(string ip)
        {

            try
            {
                double AvgHourly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Average();

                double AvgDaily =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Average();

                double AvgWeekly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Average();


                List<double> AvgRtt = new List<double>();

                AvgRtt.Insert(0, AvgHourly);
                AvgRtt.Insert(1, AvgDaily);
                AvgRtt.Insert(2, AvgWeekly);

                return AvgRtt;
                
                                
                /* Attempting to compute variance -- no success, dont have foreach loop in LINQ
                int n =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Count();
                
                double AvgHourly =
                    (from con in _db.Connections
                        where (con.ip_address == ip && con.timestamp > _startTime && con.timestamp < _endTime)
                        select con.rtt).Average();

                double total = 0;
                // Compute variance
                foreach (var con in _db.Connections)
                {
                    double delta = Math.Pow(Convert.ToDouble(con) - AvgHourly, 2);
                    total += delta;
                }

                double variance = total / n;
                
                */

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


            //Compute averages
            double avgBytesOut = BytesOut.Average();
            double avgBytesIn = BytesIn.Average();
            double avgRtt = Rtt.Average();

            double[] AvgByTime =
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
    }

