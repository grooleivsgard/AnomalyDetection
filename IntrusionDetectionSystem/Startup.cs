
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
using AutoMapper;
using DTO.IntrusionDetectionSystem;
using Models;
using System.Net;

namespace IntrusionDetectionSystem
{
    public class Startup : IStartup
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Startup> _log;
        private readonly IMapper _mapper;

        private readonly IList<Connection> _connectionDataStrructure;
        private readonly IEnumerable<IPAddress> _whiteListe;

        public Startup( HttpClient client,
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

        }

        // inspectConnection(model of mydeserialised class)
        public async Task inspectConnection() 
        {
           
            IPAddress edgeIp = IPAddress.Parse(_configuration.GetValue<String>("edgePrivateInternalIp"));
            Console.WriteLine(_connectionDataStrructure.Count());
            foreach (Connection connectionPacket in _connectionDataStrructure)
            {
                 
                if (IPAddress.Parse(connectionPacket.SourceAddress).Equals(edgeIp))
                {

                    //Is the dest_ip stored in whiteList ??
                    if(! _whiteListe.Contains(IPAddress.Parse(connectionPacket.DestinationAddress))) {
                        //Add the dst ip addr to the whiteListe 
                        _whiteListe.Append(IPAddress.Parse(connectionPacket.DestinationAddress)); 
                        _log.LogInformation($"Destination ipaddr: {connectionPacket.DestinationAddress} stored to the whiteListe"); 
                    }
                }

                else
                {

                }
            }

        }

    }
}
