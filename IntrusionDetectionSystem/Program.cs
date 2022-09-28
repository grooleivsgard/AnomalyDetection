using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Model;
namespace IntrusionDetectionSystem
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly ILogger<Program> _log;

        public Program(ILogger<Program> log)
        {
            _log = log;
        }
        private static async Task ProcessRepositories()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            string promQuery = "hosts_src_dst";
            string url = "http://10.9.10.14:9090/api/v1/query?query=" + promQuery;

            //var stringTask = client.GetStringAsync("http://10.9.10.14:9090/api/v1/query?query=" + promQuery);
            // var stringTask = client.GetStringAsync(url);
            var streamTask = client.GetStreamAsync(url);
            if (streamTask.IsCompletedSuccessfully)
            {
                Console.WriteLine("Http Get request to prometheus server was OK!");
                Root myDeserializedClass = await JsonSerializer.DeserializeAsync<Root>(await streamTask);
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
                Console.WriteLine("Http Get request to prometheus server was NOT OK!");
            }


        }
        static async Task Main(String[] args)
        {
            await ProcessRepositories();
        }

    }

}




