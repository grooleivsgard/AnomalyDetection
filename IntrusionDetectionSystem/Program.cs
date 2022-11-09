using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Models;
using Microsoft.EntityFrameworkCore;
using static Models.Endpoint;
using Microsoft.Extensions.Logging;
using IntrusionDetectionSystem.DAL;


//Di, serilog, Settings 
namespace IntrusionDetectionSystem
{
    class Program
    {
    
        async static Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder(); 
            BuildConfig(builder); 

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger(); 

            Log.Logger.Information("Application Starting"); 

            var host = Host.CreateDefaultBuilder()
                        .ConfigureServices((context, services) =>{
                            services.AddDbContext<AppDbContext>(opt => 
                            opt.UseNpgsql(@"Server=localhost;Username=postgres;Password=1234;Port=5432;Database=mydatabase"));
                            services.AddTransient<IStartup,Startup>();
                            services.AddTransient<IEndpoint,Endpoint>();  
                            services.AddScoped<IEndpointItem,EndpointItem>();  
                            services.AddHttpClient<IStartup,Startup>(); 
                            services.AddAutoMapper(typeof(Program).Assembly);
                            services.AddScoped<IList<EndpointItem>,List<EndpointItem>>();
                            services.AddScoped<IList<Endpoint>,List<Endpoint>>();
                            services.AddScoped<IList<Connection>,List<Connection>>();
                            services.AddScoped<IIntrusionRepository, IntrusionRepository>();
                            services.AddScoped<ILogger<Endpoint>, Logger<Endpoint>>();
                        })
                        .UseSerilog()
                        .Build(); 
            
            var svc = ActivatorUtilities.CreateInstance<Startup>(host.Services); 
            await svc.ProcessRepositories();
           /*var svc = ActivatorUtilities.CreateInstance<StartupPrometheusTest>(host.Services); 
            await svc.ProcessRepositories();
            var svc = ActivatorUtilities.CreateInstance<Endpoint>(host.Services); 
            svc.Run(); */
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {//Assembly.GetExecutingAssembly().Location
        //Path.GetDirectoryName(Directory.GetCurrentDirectory())
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional:false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")  ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables();
        }
    }
}




