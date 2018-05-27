using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OneScript.WebHost.BackgroundJobs
{
    public static class BackgroundJobsExtensions
    {
        public static void AddBackgroundJobsByConfiguration(this IServiceCollection services, IConfiguration config)
        {
            const string keyName = "BackgroundJobs";
            if (!config.GetChildren().Any(item => item.Key == keyName))
                return;
            
            var dbSettings = config.GetSection(keyName);
            
            var options = new OscriptBackgroundJobsOptions();
            dbSettings.Bind(options);

            AddBackgroundJobsOptions(services, options);
            
            
        }

        private static void AddBackgroundJobsOptions(IServiceCollection services, OscriptBackgroundJobsOptions options)
        {
            
            switch (options.StorageType)
            {
                case SupportedJobsStorage.MSSQLServer:
                    services.AddHangfire( c => c.UseSqlServerStorage(options.ConnectionString));
                    break;
                case SupportedJobsStorage.Postgres:
                    services.AddHangfire( c => c.UsePostgreSqlStorage(options.ConnectionString));
                    break;
                case SupportedJobsStorage.Memory:
                    services.AddHangfire( c => c.UseMemoryStorage());
                    break;
                default:
                    throw new InvalidOperationException("Unknown storage type for background jobs in configuration");
            }
            
        }
    }

    class OscriptBackgroundJobsOptions
    {
        public SupportedJobsStorage StorageType { get; set; }
        public string ConnectionString { get; set; }
    }

    enum SupportedJobsStorage
    {
        Unknown,
        MSSQLServer,
        Postgres,
        Memory
    }

}
