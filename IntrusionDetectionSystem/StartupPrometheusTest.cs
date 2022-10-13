using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace IntrusionDetectionSystem
{
    public class StartupPrometheusTest
    {

        static Meter s_meter = new Meter("Raalabs.UnknowIps", "1.0.0");
        static Counter<int> s_unknowIps = s_meter.CreateCounter<int>(name: "unknown-ips",
                                                                     unit: "IpAdrresses",
                                                                     description: "The number of unknown IP addresses trying to connecto to the edge hub ");
        public async Task ProcessRepositories()
        {
            Console.WriteLine("Prometheus exoprter starting");
/*
            using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Raalabs.UnknowIps")
                .AddPrometheusExporter(opt =>
                {
                    opt.StartHttpListener = true;
                    opt.HttpListenerPrefixes = new string[] { $"http://127.0.0.1:9184/" };
                })
                .Build();
*/
            Console.WriteLine("Press any key to exit");
            while (!Console.KeyAvailable)
            {
                // Pretend our store has a transaction each second that sells 4 hats
                Thread.Sleep(1000);
                s_unknowIps.Add(4);
            }

        }
    }
}