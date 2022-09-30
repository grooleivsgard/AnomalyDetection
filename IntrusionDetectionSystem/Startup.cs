
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
using AutoMapper;
using DTO.IntrusionDetectionSystem;

namespace IntrusionDetectionSystem
{
    public class Startup : IStartup
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Startup> _log;
        private readonly IMapper _mapper;

        public Startup(HttpClient client, ILogger<Startup> log, IConfiguration configuration, IMapper mapper)
        {
            _log = log;
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _configuration = configuration;
            _mapper = mapper; 
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
                Root myDeserializedClass = await JsonSerializer.DeserializeAsync<Root>(streamTask);
                foreach (var result in myDeserializedClass.Data.Result)
                {
                    Console.WriteLine("Connection going from: " + result.Metric.SourceAddress
                                      + " to: " + result.Metric.DestinationAddress + " at "
                                      + result.Metric.DestinationPort.Split('/')[0]
                                      + " port number: " + result.Metric.DestinationPort
                                         .Split('/')[1]);

                }
            }
            else
            {
                _log.LogError("Http Get request to prometheus server was NOT OK!");
            }

        }

        // inspectConnection(model of mydeserialised class)
        public void inspectConnection(string myDeserializedClass)
        {

        }

    }
}
