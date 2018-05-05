using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OneScript.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder();
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (location != null)
                config.AddJsonFile(Path.Combine(location, "appsettings.json"), optional: true);

            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.json", optional: true);
            config.AddEnvironmentVariables("OSWEB_");

            if (args != null)
            {
                config.AddCommandLine(args);
            }

            var configInstance = config.Build();

            var builder = new WebHostBuilder();
            var host = builder
                .UseConfiguration(configInstance)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .ConfigureLogging((hosting, logging)=>
                {
                    logging.AddConfiguration(hosting.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .Build();

            host.Run();
        }
    }
}
