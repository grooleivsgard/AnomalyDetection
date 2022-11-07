
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
        IEndpoint endpoint = new Endpoint();

        IList<IEndpointItem> _AllEndpointsFromWhiteList; // A general list that will contain all the ips from the whiteList 

        IList<IEndpoint> _EndpointToTabell;  // Endpoint Table 

        public Startup(HttpClient client,
                        ILogger<Startup> log,
                        IConfiguration configuration,
                        IMapper mapper,
                        IList<Connection> connectionDataStrructure,
                        IEnumerable<IPAddress> whiteListe,
                        //Problem is here 
                        IList<IEndpointItem> AllEndpointsFromWhiteList,
                        IList<IEndpoint> EndpointToTabell,
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
            await _db.CreateNewEndpoint("10.10.1.0", false, "mac Address 1", 9999);



            _log.LogInformation("2 -> Prometheus_Opentelemery exoprter starting");

            //Docker config: Set a http listener and expose the metrics at port 9184 
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
                    List<Result> resultCollection = myDeserializedClass.Data.Result;
                    foreach (Result result in resultCollection)
                    {
                        // The connection (Our Model) will get all its properties from the result object (Data Transfer Object) EXEPT the bytes_value

                        Connection _c = _mapper.Map<Connection>(result.Metric);

                        // The connection gets its btyes size  from the result its list of value
                        if (result.Value[0] is not null)
                        {
                            string str = result.Value[0].ToString()!;
                            _c.Bytes_value = float.Parse(str);
                            //Add the new connection instance to the collection of connections
                            _connectionDataStrructure.Add(_c);
                        }
                        else
                        {
                            _c.Bytes_value = -1;
                            _log.LogError("ProcessRepositories(): result.Value[0] is null");
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
            while (true && sw.ElapsedMilliseconds < 1200000) // run in 20 minutes 
            {
                IPAddress edgeIp = IPAddress.Parse(_configuration.GetValue<String>("edgePrivateInternalIp")!);

                //For debugging: 
                Console.WriteLine("At line: " + _connectionDataStrructure.Count());

                foreach (Connection connectionPacket in _connectionDataStrructure)
                {
                    //_log.LogInformation(connectionPacket.toString());

                    if (IPAddress.Parse(connectionPacket.SourceAddress) == edgeIp) // if  src_ip == edge_ip { the edge is talking to another ip (destination ip) }
                    {
                        // Check if this ip that the edge is talking to is stored in the whiteList 
                        bool found = FindIPAddressInWhiteList(connectionPacket.DestinationAddress);

                        if (found)
                        {
                            // Get the endpoint object that have the same ip address as the destination ip 

                            // In Memory: 
                            Endpoint endpoint = (Endpoint)_EndpointToTabell.ToList().FirstOrDefault(endpoint => endpoint.Ip == connectionPacket.DestinationAddress);
                            // In database: 
                            Endpoints endpointDB = await _db.GetEndpointByIP(connectionPacket.DestinationAddress);
                            // Create a new Connection row and add it to the list of connections that this endpoint has in database 
                            Connections cnxDB = new Connections();

                            if (endpoint is not null)
                            {
                                bool stateOk = StatesHandler.HandleState(endpoint!.Status, 1);

                                if (stateOk)
                                {
                                    endpoint.Status = 1;
                                    //endpoint.Bytes_out = connectionPacket.Bytes_value;
                                    cnxDB.bytes_out = (long)connectionPacket.Bytes_value;
                                    timer.Start();
                                    //Save bytes_out to database

                                }
                                else _log.LogWarning("State not Allowed");
                            }

                            else
                            {
                                _log.LogWarning("Internal error: Ip is found in whitelist but not declared as an object");
                            }

                        }

                        // if (dest is in whitelist ) 

                        else if (!found)
                        {
                            _log.LogCritical("IP not in whitelist");
                            //legg inn i unknown db
                        }
                    }

                    else // src_ip != edge_ip {In this case it is not the edge that is talking but another ip }
                    {
                        // Check if that ip is stored in the white list 
                        bool found = FindIPAddressInWhiteList(connectionPacket.SourceAddress);

                        if (found)
                        {
                            // Call endpoint and get the endpoint object that have the same ip address as the destination ip 

                            // In Memory: 
                            Endpoint endpoint = (Endpoint)_EndpointToTabell.ToList().FirstOrDefault(endpoint => endpoint.Ip == connectionPacket.SourceAddress);

                            // In database: 
                            Endpoints endpointDB = await _db.GetEndpointByIP(connectionPacket.SourceAddress);
                            Connections cnxDB; 

                            if (endpointDB.connections.Count() == 0 )
                            {
                                // If endpoint has no connections yet 
                                // Create a new Connection row and add it to the list of connections that this endpoint has in database 
                                 cnxDB = new Connections();
                            }

                            else cnxDB = endpointDB.connections.LastOrDefault()!; 

                            
                            if (endpoint is not null)
                            {
                                bool stateOk = StatesHandler.HandleState(endpoint!.Status, 2);
                                if (stateOk)
                                {
                                    endpoint.Status = 2;
                                    endpoint.Bytes_in = (long)connectionPacket.Bytes_value;
                                    timer.Stop();
                                    endpoint.RTT = timer.Elapsed;
                                    // Statisktiik 
                                    // KjørStatistikk (endpoint)
                                    // Nullstill endpoint objekt  
                                    ResetEndpoint(endpoint);
                                }
                                else _log.LogWarning("State not Allowed");
                            }

                            else
                            {
                                _log.LogWarning("Internal error: Ip is found in whitelist but not declared as an object");
                            }

                        }

                        else
                        {
                            _log.LogWarning("Src not in whiteListe");
                            unkown++;
                            s_unknowIps.Add(1);
                            //legg inn i unknown db
                        }


                    }
                }
                Thread.Sleep(1000);
            }
        }


        void checkStatistics()
        {

        }

        private bool FindIPAddressInWhiteList(string _ipAddress)
        {
            // Check if the The ipAddress we are looking for, is registred in the whiteList 
            bool IpFoundInWhiteList = _AllEndpointsFromWhiteList.Any(end => end.IP == _ipAddress);

            return IpFoundInWhiteList;
        } // FindIPAddressInWhiteList:  checks if the whiteList contains a certain IpAddress


        public async Task<List<Endpoints>> RetrieveAll()
        {
            List<Endpoints> allEndpoints = await _db.GetAllEndpoints();
            return allEndpoints;
        }

        public void ResetEndpoint(Endpoint endpoint)
        {
            endpoint.Bytes_in = 0;
            endpoint.Bytes_out = 0;
            endpoint.RTT = null;
            endpoint.Status = 0;
        }

    }


}
