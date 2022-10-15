
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
using AutoMapper;
using DTO.IntrusionDetectionSystem;
using Models;
using System.Net;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry;
using System.Diagnostics;


namespace IntrusionDetectionSystem
{
    public class Startup : IStartup
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Startup> _log;
        private readonly IMapper _mapper;
        private MeterProvider _meterProvider;

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
        private int key = 0;

        private Stopwatch sw = new Stopwatch();

        public Startup(HttpClient client,
                        ILogger<Startup> log,
                        IConfiguration configuration,
                        IMapper mapper,
                        IList<Connection> connectionDataStrructure,
                        IEnumerable<IPAddress> whiteListe
                       )
        {

            _log = log;
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _configuration = configuration;
            _mapper = mapper;
            _connectionDataStrructure = connectionDataStrructure;
            _whiteListe = whiteListe;

        }
        public async Task ProcessRepositories()
        {
            // Call prometheusexporter function to expose uknown_ips Metric
            s_unknowIps.Add(1);

            _log.LogInformation("2 -> Prometheus_Opentelemery exoprter starting");
            using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
               .AddMeter("Raalabs.UnknowIps")
               .AddPrometheusExporter(opt =>
               {
                   opt.StartHttpListener = true;
                   opt.HttpListenerPrefixes = new string[] { $"http://*:9184/" };
               })
               .Build();

            using MeterProvider meterProvider_1 = Sdk.CreateMeterProviderBuilder()
               .AddMeter("Raalabs.UnknowIps")
               .AddPrometheusExporter(opt =>
               {
                   opt.StartHttpListener = true;
                   opt.HttpListenerPrefixes = new string[] { $"http://localhost:9184/" };
               })
               .Build();
            s_unknowIps.Add(5);


            _client.DefaultRequestHeaders.Accept.Clear();
            string promQuery = "hosts_src_dst";
            string url = _configuration.GetValue<String>("url") + promQuery;


            var response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                Stream streamTask = await response.Content.ReadAsStreamAsync();
                _log.LogInformation("Http Get request to prometheus server was OK!");
                Root myDeserializedClass = await JsonSerializer
                                           .DeserializeAsync<Root>(streamTask);
                
                List<Result> resultCollection = myDeserializedClass.Data.Result;

                /*if (resultCollection is not null ) Console.WriteLine("resultcollection is not null"); 
                    else  Console.WriteLine("resultcollection is  null");

                if (resultCollection[0]==null) Console.WriteLine("_c is not null"); 
                    else  Console.WriteLine("_c is  null");*/
                /*resultCollection.ForEach(

                    Connection c = new Connection(); 
                    //result => _connectionDataStrructure.Add(Connection c = _mapper.Map<Connection>(result.Metric)), result => 
                    _connectionDataStrructure.Add(c); 
                    
                    );
*/
                foreach (Result result in resultCollection)
                {
                    Connection _c = _mapper.Map<Connection>(result.Metric); 
                    // The connection (Our Model) will get all its properties from the result object (Data Transfer Object)
                    //_c = ; 
                    //For debugging 
                    if (_c is not null ) Console.WriteLine("_c is not null"); 
                    else  Console.WriteLine("_c is  null"); 

                    if (result.Value is not null )  Console.WriteLine("result value is not null"); 
                    else  Console.WriteLine("result value is  null");
                    
                    // The connection gets its btyes size  from the result its list of value
                    if (result.Value != null) 
                    {
                      _c.Bytes_value =  Double.Parse(result.Value[1].ToString());  
                      Console.WriteLine(" _c.Bytes_value is: "+  _c.Bytes_value);
                    }
                    else 
                    {
                        _c.Bytes_value = -1; 
                        _log.LogError("ProcessRepositories(): result.Value[0] is null"); 
                    }
                    Console.WriteLine(" 111 : "+_c.toString()); 
                    Console.WriteLine(myDeserializedClass.toString());
                    
                }
                
/*
                foreach (var item in _connectionDataStrructure) 
                
                {
                    foreach (var res in resultCollection)
                    {
                        item.Bytes_value = res.Value[0]; 
                    }

                }*/

                await inspectConnection();


            }

            else
            {
                _log.LogError("Http Get request to prometheus server was NOT OK!");
            }

        }

        // inspectConnection(model of mydeserialised class)
        public async Task inspectConnection()
        {

            sw.Start();
            while (true && sw.ElapsedMilliseconds < 1200000) // run in 20 minutes 
            {
                IPAddress edgeIp = IPAddress.Parse(_configuration.GetValue<String>("edgePrivateInternalIp")!);
                
                Console.WriteLine(_connectionDataStrructure.Count());

                foreach (Connection connectionPacket in _connectionDataStrructure)
                {
                     _log.LogInformation(connectionPacket.toString());
/*
                    if (IPAddress.Parse(connectionPacket.SourceAddress).Equals(edgeIp)) // src_ip == edge_ip ??
                    {

                        //Is the dest_ip stored in whiteList ??
                        if (!_whiteListe.Contains(IPAddress.Parse(connectionPacket.DestinationAddress)))
                        {
                            //Add the dst ip addr to the whiteListe 
                            _whiteListe.Append(IPAddress.Parse(connectionPacket.DestinationAddress));
                            _log.LogInformation($"Destination ipaddr: {connectionPacket.DestinationAddress} stored to the whiteListe");
                            s_unknowIps.Add(1);
                        }

                        // to_state = 1;
                        /* checkState(IP) //returns int curr_state;
                            StatesHandles(curr_state, to_state)  //returns true
                            setState (IP, to_state)
                        */
                        /*
                    }

                    else // src_ip != edge_ip 
                    {
                        if (!_whiteListe.Contains(IPAddress.Parse(connectionPacket.SourceAddress))) // ip_adr not stored in the whitelist  
                        {
                            unkown++;
                            uknownConnectionsOverTime.Add(key, new DateTimeOffset(DateTime.UtcNow));
                            key++;
                            s_unknowIps.Add(1);
                        }

                        // to_state = 2;
                    }*/
                }
                Thread.Sleep(1000);
               

            }

        }

        public bool checkState(int state)
        {
            // vi har den stateshandler
            return false;
        }
        public void updateState()
        {
            //this._
        }

    }


}

