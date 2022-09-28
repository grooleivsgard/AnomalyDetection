using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using static IntrusionDetectionSystem.PrometheusParser; 
using  Microsoft.Extensions.Logging; 
using Model; 
namespace IntrusionDetectionSystem
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient(); 
        private readonly ILogger <Program> _log; 

        public Program (ILogger<Program> log) 
        {
            _log = log; 
        }
        private static async Task ProcessRepositories()
        {
            client.DefaultRequestHeaders.Accept.Clear(); 
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json")); 
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter"); 
            string promQuery = "hosts_src_dst"; 
            string url = "http://10.9.10.14:9090/api/v1/query?query=hosts_src_dst"; 

            //var stringTask = client.GetStringAsync("http://10.9.10.14:9090/api/v1/query?query=" + promQuery);
           // var stringTask = client.GetStringAsync(url);
           var streamTask = client.GetStreamAsync(url);
           Root myDeserializedClass = await JsonSerializer.DeserializeAsync<Root>(await streamTask); 
           // var metrics = await JsonSerializer.DeserializeAsync<List<Metrics>>(await streamTask);
           // var msg = await stringTask; 
           // Console.Write(msg); 
           // Parse(await stringTask); 

           foreach (var result in myDeserializedClass.data.result) {
               // Console.WriteLine("Connection going from: " + result.metric.a_srcAddr + " to: " + result.metric.b_dstAddr +  " at " + result.metric.c_dstPort.Split('/')[0] + " port number: " + result.metric.c_dstPort.Split('/')[1]); 
               whileListe.add(result.metric.a_srcAddr);
           }
          
        }
        static async Task Main(String[] args)
        {
            await ProcessRepositories();
        }

    }

}




