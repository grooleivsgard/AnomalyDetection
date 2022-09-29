
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Model;
using System.Net.Http; 
public class Startup {
        private  readonly HttpClient _client; 
        private readonly IConfiguration _configuration;
        private readonly ILogger<Startup> _log;

        public Startup(HttpClient client, ILogger<Startup> log, IConfiguration configuration)
        {
            _log = log;
            _client = client; 
            _configuration = configuration; 
        }
        public async Task ProcessRepositories()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            string promQuery = "hosts_src_dst";
            string url = "http://10.9.10.14:9090/api/v1/query?query=" + promQuery;


            var response = await _client.GetAsync(url);
            if ( response.IsSuccessStatusCode)
            {
                Stream streamTask = await response.Content.ReadAsStreamAsync();   
                Console.WriteLine("Http Get request to prometheus server was OK!");
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
                                _log.LogInformation("Http Get request to prometheus server was OK!");

                Console.WriteLine("Http Get request to prometheus server was NOT OK!");
            }


        }

}