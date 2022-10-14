
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
            // PrometheusExporter();
            //s_unknowIps.Add(1);

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
            //PrometheusExporter();
            s_unknowIps.Add(1);
            
            /*using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("Raalabs.UnknowIps")
            .AddPrometheusHttpListener()
            .Build();
            ;*/
            /*
             using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("Raalabs.UnknowIps")
            .AddPrometheusExporter(opt =>
            {
                Console.WriteLine("Jeg er inn 1!!!");
                opt.StartHttpListener = true;
                opt.HttpListenerPrefixes = new string[] { $"http://localhost:9184/" };
            })
            .Build();*/
             using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Raalabs.UnknowIps")
                .AddPrometheusExporter(opt =>
                {
                    Console.WriteLine("Jeg er inn!!!");
                    opt.StartHttpListener = true;
                    opt.HttpListenerPrefixes = new string[] { $"http://*:9184/" };
                })
                .Build();
            s_unknowIps.Add(5);
            _log.LogInformation("2 -> Prometheus exoprter starting");

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

                resultCollection.ForEach(result => _connectionDataStrructure.Add(_mapper.Map<Connection>(result.Metric)));

                await inspectConnection();


            }

            else
            {
                _log.LogError("Http Get request to prometheus server was NOT OK!");
            }
            //PrometheusExporter(); 

        }

        // inspectConnection(model of mydeserialised class)
        public async Task inspectConnection()
        {
            // Call prometheusexporter function to expose uknown_ips Metric 


            // sigbjorn endra her

            //PrometheusExporter(); 

            sw.Start();
            while (true && sw.ElapsedMilliseconds < 1200000) // run in 20 minutes 
            {
                IPAddress edgeIp = IPAddress.Parse(_configuration.GetValue<String>("edgePrivateInternalIp")!);

                Console.WriteLine(_connectionDataStrructure.Count());

                foreach (Connection connectionPacket in _connectionDataStrructure)
                {

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
                    }
                }
                Thread.Sleep(10000);

            }

        }

       /* public void PrometheusExporter()
        {
            _log.LogInformation("Prometheus exoprter starting");


            MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Raalabs.UnknowIps")
                .AddPrometheusExporter(opt =>
                {
                    Console.WriteLine("Jeg er inn!!!");
                    opt.StartHttpListener = true;
                    opt.HttpListenerPrefixes = new string[] { $"http://127.0.0.1:9184/" };
                })
                .Build();

            _meterProvider = meterProvider;

            s_unknowIps.Add(5);
            _log.LogInformation("2 -> Prometheus exoprter starting");

            meterProvider.Dispose();
        }*/

    public bool checkState(int state)
    {
       // boo
        return false;
    }
    public void updateState()
    {
       //this._
    }

    }


}
