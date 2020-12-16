/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
#if NET461
using Microsoft.AspNetCore.Hosting.WindowsServices;
#endif
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

            var contentRoot = GetContentRoot(args);

            config.SetBasePath(contentRoot);
            config.AddJsonFile("appsettings.json", optional: true);
            config.AddEnvironmentVariables("OSWEB_");
            config.AddEnvironmentVariables("ASPNETCORE_");

            if (args != null)
            {
                var argMappings = new Dictionary<string, string>()
                {
                    { "--ContentRoot", "Hosting:ContentRoot" },
                    { "--RunAsService", "Hosting:RunAsService" },
                    { "--Urls", "Hosting:Urls" }
                };
                config.AddCommandLine(args, argMappings);
                argMappings = new Dictionary<string, string>()
                {
                    { "--ContentRoot", "ContentRoot" },
                    { "--RunAsService", "RunAsService" },
                    { "--Urls", "Urls" }
                };
                config.AddCommandLine(args, argMappings);
            }

            var configInstance = config.Build();
            var builder = new WebHostBuilder();
            var options = ConfigureHostingMode(builder, configInstance);

            builder.UseConfiguration(configInstance)
                .UseKestrel(opts => opts.AllowSynchronousIO = true)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .ConfigureLogging((hosting, logging) =>
                {
                    logging.AddConfiguration(hosting.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            var host = builder.Build();

#if NET461
            if (options.RunAsService)
                host.RunAsService();
            else
                host.Run();
#else
            host.Run();
#endif

        }

        private static HostingOptions ConfigureHostingMode(WebHostBuilder builder, IConfigurationRoot config)
        {
            var options = new HostingOptions();
            config.GetSection("Hosting").Bind(options);

            if (options.ContentRoot == null)
            {
                options.ContentRoot = options.RunAsService
                    ? Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
                    : Directory.GetCurrentDirectory();
            }

            if (options.Urls != null)
                builder.UseSetting("urls", options.Urls);

            builder.UseContentRoot(options.ContentRoot);
            if (options.WebRoot != null)
                builder.UseWebRoot(options.WebRoot);

            return options;
        }

        private class HostingOptions
        {
            public string Urls { get; set; }
            public bool RunAsService { get; set; }
            public string ContentRoot { get; set; }
            public string WebRoot { get; set; }

        }

        private static string GetContentRoot(string[] args)
        {
            var config = new ConfigurationBuilder();

            if (args != null)
            {
                config.AddCommandLine(args);
            }

            var configInstance = config.Build();

            string contentRoot = configInstance.GetValue<string>("ContentRoot", "");

            if (contentRoot == "")
                contentRoot = Directory.GetCurrentDirectory();

            return contentRoot;

        }
    }
}
