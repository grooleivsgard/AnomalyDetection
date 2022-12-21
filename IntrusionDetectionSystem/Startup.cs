
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AutoMapper;
using DTO.IntrusionDetectionSystem;
using Models;
using System.Net;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry;
using System.Diagnostics;
using IntrusionDetectionSystem.DAL;

namespace IntrusionDetectionSystem
{
    public class Startup : IStartup
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Startup> _log;
        private readonly IMapper _mapper;
        private MeterProvider _meterProvider;

        private readonly IIntrusionRepository _db;

        private readonly IList<Connection> _connectionDataStrructure;
        private readonly IEnumerable<IPAddress> _whiteListe;
        private int unkown = 0;
        // Unknown connections with timestamp 
        //private readonly struct uknownConnection<Integer,timestamp> {}
        private IDictionary<int, DateTimeOffset> uknownConnectionsOverTime = new Dictionary<int, DateTimeOffset>();
        static Meter s_meter = new Meter("Raalabs.UnknowIps", "1.0.0");
        static Counter<int> s_unknowIps = s_meter.CreateCounter<int>(name: "unknown-ips",
                                                                     unit: "IpAdrresses",
                                                                     description: "The number of unknown IP addresses trying to connecto to the edge hub ");

        private Stopwatch sw = new Stopwatch();
        private Stopwatch timer = new Stopwatch();

        //Make an instance of the non static class Endpoint 
        Endpoint endpoint = new Endpoint();

        IList<EndpointItem> _AllEndpointsFromWhiteList; // A general list that will contain all the ips from the whiteList 

        IList<Endpoint> _EndpointToTabell;  // Endpoint Table 

        public Startup(HttpClient client,
                        ILogger<Startup> log,
                        IConfiguration configuration,
                        IMapper mapper,
                        IList<Connection> connectionDataStrructure,
                        IEnumerable<IPAddress> whiteListe,
                        //Problem is here 
                        IList<EndpointItem> AllEndpointsFromWhiteList,
                        IList<Endpoint> EndpointToTabell,
                        IIntrusionRepository db
                       )
        {

            _log = log;
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _configuration = configuration;
            _mapper = mapper;
            _connectionDataStrructure = connectionDataStrructure;
            _whiteListe = whiteListe;
            // Call Run method in Endpoint.cs class that gets the whiteList and creates a new Table of all ips in The whiteList
            _AllEndpointsFromWhiteList = AllEndpointsFromWhiteList = endpoint.LoadJson();
            _EndpointToTabell = EndpointToTabell = endpoint.EndpointToTabell();
            _db = db;

        }
        public async Task ProcessRepositories()
        {
            // Call prometheusexporter function to expose uknown_ips Metric
            s_unknowIps.Add(1);

            _log.LogInformation("2 -> Prometheus_Opentelemery exoprter starting");

            //For Docker config: Set a http listener and expose the metrics at port 9184 
            using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
               .AddMeter("Raalabs.UnknowIps")
               .AddPrometheusExporter(opt =>
               {
                   opt.StartHttpListener = true;
                   opt.HttpListenerPrefixes = new string[] { $"http://*:9184/" };
               })
               .Build();
            


            //Local config: Set a http listener and expose the metrics at port 9184 at localhost
            using MeterProvider meterProvider_1 = Sdk.CreateMeterProviderBuilder()
               .AddMeter("Raalabs.UnknowIps")
               .AddPrometheusExporter(opt =>
               {
                   opt.StartHttpListener = true;
                   opt.HttpListenerPrefixes = new string[] { $"http://localhost:9184/" };
               })
               .Build();
            s_unknowIps.Add(5);


            // Get the metrics data from the prometheus server api 

            _client.DefaultRequestHeaders.Accept.Clear();
            string promQuery = "hosts_src_dst";
            string url = _configuration.GetValue<String>("url") + promQuery;


            var response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                Stream streamTask = await response.Content.ReadAsStreamAsync();
                _log.LogInformation("Http Get request to prometheus server was OK!");
                Root myDeserializedClass = await JsonSerializer
                                           .DeserializeAsync<Root>(streamTask) ?? throw new ArgumentNullException("streamTask is null. Cannot convert a null object to a non-nullable object(Root)");

                if (myDeserializedClass is not null)
                {
                    List<Result> resultCollection = myDeserializedClass.Data!.Result!;
                    foreach (Result result in resultCollection!)
                    {
                        // The connection (Our Model) will get all its properties from the result object (Data Transfer Object) EXEPT the bytes_value

                        Connection _c = _mapper.Map<Connection>(result.Metric);

                        // The connection gets its btyes size  from the result its list of value
                        if (result.Value![0] is not null)
                        {
                            string str = result.Value[0].ToString()!;
                            _c.Bytes_value = float.Parse(str);
                            //Add the new connection instance to the collection of connections
                            _connectionDataStrructure.Add(_c);
                           // Console.WriteLine("At line 136: " +_c.toString()); 
                        }
                        else
                        {
                            _c.Bytes_value = -1;
                            _log.LogError("ProcessRepositories(): result.Value[0] is null");
                            Console.WriteLine("At line 141: " + "ProcessRepositories(): result.Value[0] is null" ); 
                        }
                    }
                }

                else if (myDeserializedClass is null)
                {
                    _log.LogError("My decerialised class is null. Error deserialising json object to a DecerialisedClass object");
                }

                await inspectConnection();
            }

            else
            {
                _log.LogError("Http Get request to prometheus server was NOT OK!");
            }

        }


        public async Task inspectConnection()
        {

            //Start the stopwatch 
            sw.Start();
            while (true ) 
            {
                string edgeIp = _configuration.GetValue<String>("edgePrivateInternalIp")!;

                //For debugging: 
                Console.WriteLine("At line 184 in code we have  : " + _connectionDataStrructure.Count() + " Connection strings");

                foreach (Connection connectionPacket in _connectionDataStrructure)
                {

                    if (connectionPacket.SourceAddress == edgeIp) // if  src_ip == edge_ip {IF the edge is talking to another ip (destination ip) }
                    {
                        // Check if this ip that the edge is talking to is stored in the whiteList 
                        bool found = FindIPAddressInWhiteList(connectionPacket.DestinationAddress!);
                        
                        if (found)
                        {
                            // Get the endpoint object that have the same ip address as the destination ip 

                            // In Memory: 
                            Endpoint endpoint =  _EndpointToTabell!.ToList().FirstOrDefault(endpoint => endpoint.Ip == connectionPacket.DestinationAddress) ?? throw new ArgumentNullException("Cannot find the endpoint object that have the same ip address as the destination ip");

                            

                            if (endpoint is not null) 
                            { 
                                bool match = MatchIPtoMac(connectionPacket.DestinationMac ?? throw new ArgumentNullException ("ConnectionPacket its DestinationMac is null"), endpoint.Mac);
                                
                                if (match)
                                {
                                    bool stateOk = StatesHandler.HandleState(endpoint!.State, 1);
                                    endpoint.Bytes_out = (long) connectionPacket.Bytes_value;
                                    if (stateOk)
                                    {
                                        endpoint.State = 1;
                                        timer.Start();
                                    }
                                    else {
                                        if (sw.ElapsedMilliseconds > 60000) 
                                        {
                                            _log.LogWarning("State not Allowed catched for ip: " + endpoint.Ip + ". This ip was trying to go from state " + endpoint!.State + " to state 1");
                                            endpoint.isAnomolous = true; 
                                            endpoint.anomalityReport += " Deviation: Possibly something wrong is going on. Catched Unallowed state. State not Allowed catched for ip: " + endpoint.Ip + ". This ip was trying to go from state " + endpoint!.State + " to state 1.";
                                        }
                                    }
                                }
                                else
                                {
                                    _log.LogWarning("The ip " + connectionPacket.DestinationAddress + " doesen't match with the corresponding mac address");
                                }
                            }

                       

                        }// if (dest is in whitelist ) 

                        

                        else if (!found)
                        {
                            _log.LogCritical("The edge is talking to an unkown ip " + connectionPacket.DestinationAddress);
                            
                        }
                    }

                    // src_ip != edge_ip {In this case it is not the edge that is talking but another ip is talking to the edge }
                    else 
                    {
                        // Check if that ip is stored in the whitelist 
                        bool found = FindIPAddressInWhiteList(connectionPacket.SourceAddress!);

                        if (found)
                        {
                            // Call endpoint and get the endpoint object that have the same ip address as the destination ip 
                            
                            // In Memory: 
                            Endpoint endpoint = _EndpointToTabell.ToList().FirstOrDefault(endpoint => endpoint.Ip == connectionPacket!.SourceAddress!) ?? throw new ArgumentNullException("Cannot find the endpoint object that have the same ip address as the destination ip");
                            
                            // In database: 
                            Endpoints endpointDB = await _db.GetEndpointByIP(connectionPacket.SourceAddress!);
                            
                            if (endpointDB is null) 
                            {
                                  /* ** Ip address is in memory whiteList but not saved to Database yet 
                                     ** Save it to Database  */ 
                                bool created = await _db.CreateNewEndpointInDb(endpoint!.Ip, true, endpoint!.Mac); 
                                if (created)
                                {
                                   endpointDB = await _db.GetEndpointByIP(connectionPacket.SourceAddress!);
                                   _log.LogInformation("New endpoint is created at database"); // just for debugging  
                                } 
                                else if (!created) 
                                {
                                    _log.LogDebug("Error while trying to save new endpoint to db");
                                }
                            }

                            if (endpoint is not null)
                            {
                                // This connection object will be saved to the corresponding endpoint list of connections.
                                Connections newConnection = new Connections();
            
                                bool match = MatchIPtoMac(connectionPacket.SourceMac!, endpoint.Mac);

                                

                                if (match)
                                {
                                    bool stateOk = StatesHandler.HandleState(endpoint!.State, 2);
                                    if (stateOk)
                                    {
                                        endpoint.State = 2;
                                        timer.Stop();
                                        // endpoint.RTT = timer.Elapsed;

                                        // CHECK IF THE STATICTICS REVEALS SOME ANOMALITY 
                                        if (await isAnomolous(endpoint))
                                        
                                        {
                                            // log anomolous packet
                                            endpoint.isAnomolous = true; 
                                            _log.LogInformation("endpoint is statsitically anomolous"); 
                                            endpoint.anomalityReport += " Endpoint with ip: " + endpoint.Ip + " is suspected to be statistically suspecious"; 
                                        }
                                        
                                        ResetEndpoint(endpoint);
                                    }
                                    else
                                    {
                                        _log.LogWarning("State not Allowed");
                                        endpoint.isAnomolous = true; 
                                        endpoint.anomalityReport += " IP " + endpoint.Ip + " is trying to illegally from state " + endpoint.State + " to state 2";
                                    } 
                                    
                                }
                                else
                                {
                                    endpoint.isAnomolous = true; 
                                    endpoint.anomalityReport += " Ip " + endpoint.Ip + " does not match with this mac address " + endpoint.Mac;  
                                    
                                    _log.LogWarning("Error: Ip does not match with mac address");
                                }

                                newConnection.bytes_out = endpoint.Bytes_out; 
                                endpoint.Bytes_in = endpoint.Bytes_in;
                                newConnection.anomaly = endpoint.isAnomolous;
                                newConnection.anomalityReport = endpoint.anomalityReport;

                                int connectionID = await _db.AddNewConnectionToEndpoint(newConnection, endpointDB!); 
        
                            }
                                

                            
                        }

                        else
                        {
                            
                            _log.LogWarning("Unknown Ip " + connectionPacket.SourceAddress + " is trying to talk to edge");
                            unkown++;
                            s_unknowIps.Add(1);
                            //legg inn i unknown db
                          /*  Endpoint MalisciousEndpoint = new Endpoint(); 
                            MalisciousEndpoint.Ip = connectionPacket.SourceAddress; 
                            MalisciousEndpoint.Bytes_in = (long) connectionPacket.Bytes_value; 

                            //Add more Values etterhvert 
                            await SaveConnectionToDatabase(MalisciousEndpoint); */

                        }


                    }
                }
                Thread.Sleep(1000);
            }
        }

        private bool FindIPAddressInWhiteList(string _ipAddress)
        {
            // Check if the The ipAddress we are looking for, is registred in the whiteList 
            bool IpFoundInWhiteList = _AllEndpointsFromWhiteList.Any(end => end.IP == _ipAddress);
            
            return IpFoundInWhiteList;
        } // FindIPAddressInWhiteList:  checks if the whiteList contains a certain IpAddress


        private bool MatchIPtoMac(string connectionMac, string endpointMac)
        {
            bool match = connectionMac == endpointMac;
            return match;
            
        }
        
        public async Task<List<Endpoints>> RetrieveAll()
        {
            List<Endpoints> allEndpoints = await _db.GetAllEndpoints();
            return allEndpoints;
        }
        
        /**
         * Method calls DB-methods from IntrusionRepository to retrieve
         * values (standard deviation) for hour, day and week.
         * The SD values are compared with object Endpoint values (current)
         * in Statistics and is marked as anomalous = true or anomalous = false.
         */
        public async Task<bool> isAnomolous(Endpoint endpoint) //Hente inn objekt endpoint som er ferdig utfylt
        {

            DateTime hour = DateTime.Now.AddHours(-1);
            long timestampHour = hour.Ticks;
            
            DateTime day = DateTime.Now.AddHours(-24);
            long timestampDay = day.Ticks;
            
            DateTime week = DateTime.Now.AddHours(-168);
            long timestampWeek = week.Ticks;
            
            bool anomalous = false;


            // Retrieve BYTES OUT from DB
            List<long> dbBytesOutLastHour = await _db.GetParamValuesByTime(endpoint.Ip, "bytes_out", timestampHour); // returns list of bytes out for last hour
            List<long> dbBytesOutLastDay = await _db.GetParamValuesByTime(endpoint.Ip, "bytes_out", timestampDay); // returns list of bytes out for last day
            List<long> dbBytesOutLastWeek = await _db.GetParamValuesByTime(endpoint.Ip, "bytes_out", timestampWeek); // returns list of bytes out for last week
            
            // Retrieve BYTES IN from DB
            List<long> dbBytesInLastHour = await _db.GetParamValuesByTime(endpoint.Ip, "bytes_in", timestampHour); // returns list of bytes in for last hour
            List<long> dbBytesInLastDay = await _db.GetParamValuesByTime(endpoint.Ip, "bytes_in", timestampDay); // returns list of bytes in for last day
            List<long> dbBytesInLastWeek = await _db.GetParamValuesByTime(endpoint.Ip, "bytes_in", timestampWeek); // returns list of bytes in for last week
            
            // Retrieve RTT from DB
            List<long> dbRttLastHour = await _db.GetParamValuesByTime(endpoint.Ip, "rtt", timestampHour); // returns list of rtt for last hour
            List<long> dbRttLastDay = await _db.GetParamValuesByTime(endpoint.Ip, "rtt", timestampDay); // returns list of rtt for last day
            List<long> dbRttLastWeek = await _db.GetParamValuesByTime(endpoint.Ip, "rtt", timestampWeek); // returns list of rtt for last week

            // Calculate AVERAGE and STANDARD DEVIATION
            List <double> statsBytesOut = Statistics.calcData(dbBytesOutLastHour, dbBytesOutLastDay, dbBytesOutLastWeek); // returns list og avg and sd for hour, day, week
            List <double> statsBytesIn = Statistics.calcData(dbBytesInLastHour, dbBytesInLastDay, dbBytesInLastWeek);
            List <double> statsRtt = Statistics.calcData(dbRttLastHour, dbRttLastDay, dbRttLastWeek);
            
            //Compare CURRENT VALUE with AVERAGE, STANDARD DEVIATION and Z-SCORE
            bool byteOutIsOutlier = Statistics.compareValues(statsBytesOut, endpoint.Bytes_out);
            bool byteInIsOutlier = Statistics.compareValues(statsBytesIn, endpoint.Bytes_in);
            bool rttIsOutlier = Statistics.compareValues(statsRtt, endpoint.RTT);
   
            //If ANY of the values are OUTLIERS, ANOMALOUS is TRUE
            if (byteOutIsOutlier)
            {
                anomalous = true;
            }

            if (byteInIsOutlier)
            {
                anomalous = true;
            }
            
            if (rttIsOutlier)
            {
                anomalous = true;
            }
            
            return anomalous;
        }
        
 
        public void ResetEndpoint(Endpoint endpoint)
        {
            endpoint.Bytes_in = 0;
            endpoint.Bytes_out = 0; 
            endpoint.RTT = 0;
            endpoint.State = 0;
            endpoint.isAnomolous = false; 
            endpoint.anomalityReport = ""; 
            
        }

        public async Task<int> SaveConnectionToDatabase(Endpoint endpoint) 
        {
            string ip = endpoint.Ip; 
            Endpoints endpointDB = await _db.GetEndpointByIP(ip);     

            if (endpointDB is null )   //If endpointDB was not stored to the database before        
            {
                    //Check if the ip string is stored in the whitelist 
                    bool whiteList = FindIPAddressInWhiteList(ip);
                    await _db.CreateNewEndpointInDb(ip,whiteList,"mock mac");
                    /* 
                     ** Save new endpoint to Endpoints Table in Database
                     ** Retrieve it again 
                    */
                   
            }  
            endpointDB = await _db.GetEndpointByIP(ip);  
            Connections newConnection = new Connections(); 
            newConnection.bytes_in = endpoint.Bytes_in; 
            newConnection.bytes_out = endpoint.Bytes_out; 
            newConnection.ip_address = ip; 
            newConnection.rtt = endpoint.RTT; 
            newConnection.timestamp = DateTime.Now.Ticks; 

            // Return the Id of the new Connection and save it to Connections Table 
            int connectionID = await _db.AddNewConnectionToEndpoint(newConnection, endpointDB); 
            return connectionID; 

        }//SaveConnectionToDatabase() 

    }


}
