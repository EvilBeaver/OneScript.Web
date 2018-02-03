using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace OneScript.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var confBuilder = new ConfigurationBuilder();
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            confBuilder.AddJsonFile(Path.Combine(location, "appsettings.json"), optional: true);
            confBuilder.SetBasePath(Directory.GetCurrentDirectory());
            confBuilder.AddJsonFile("appsettings.json", optional: true);
            var cfg = confBuilder.Build();

            var host = new WebHostBuilder()
                .UseConfiguration(cfg)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
