/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OneScript.WebHost.Database;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.HostedScript.Library.Tasks;

namespace OneScript.WebHost.BackgroundJobs
{
    public static class BackgroundJobsExtensions
    {
        public static void AddBackgroundJobsByConfiguration(this IServiceCollection services, IConfiguration config)
        {
            const string keyName = "BackgroundJobs";
            if (!config.GetChildren().Any(item => item.Key == keyName))
                return;
            
            var jobsSettings = config.GetSection(keyName);
            services.Configure<OscriptBackgroundJobsOptions>(jobsSettings);
            AddBackgroundJobsOptions(services, config);
        }

        private static void AddBackgroundJobsOptions(IServiceCollection services, IConfiguration config)
        {
            // hangfire не поддерживает штатное конфигурирование с помощью IServiceProvider
            // см. https://github.com/HangfireIO/Hangfire/issues/1178
            //но когда начнет - нам не придется делать переосмысливание json

            services.AddHangfire(hfGlobalConfig =>
            {
                var jobsSettings = config.GetSection("BackgroundJobs");
                var options = new OscriptBackgroundJobsOptions();
                
                jobsSettings.Bind(options);
                
                switch (options.StorageType)
                {
                    case SupportedJobsStorage.Database:
                        var dbOptionsSection = config.GetSection(DatabaseExtensions.ConfigSectionName);
                        var dbOptions = new OscriptDbOptions();
                        dbOptionsSection.Bind(dbOptions);
                        switch (dbOptions.DbType)
                        {
                            case SupportedDatabase.MSSQLServer:
                                hfGlobalConfig.UseSqlServerStorage(dbOptions.ConnectionString);
                                break;
                            case SupportedDatabase.Postgres:
                                hfGlobalConfig.UsePostgreSqlStorage(dbOptions.ConnectionString);
                                break;
                            default:
                                throw new InvalidOperationException("Database for Background Jobs is not configured");
                        }
                        break;
                    case SupportedJobsStorage.Memory:
                        hfGlobalConfig.UseMemoryStorage();
                        break;
                    default:
                        throw new InvalidOperationException("Unknown storage type for background jobs in configuration");
                }
                
            });
        }

        public static void PrepareBgJobsEnvironment(IServiceProvider services, RuntimeEnvironment environment)
        {
            var hfOptions = services.GetService<IOptions<OscriptBackgroundJobsOptions>>().Value;
            if (hfOptions != null)
            {
                var jobsManager = new ScheduledJobsManagerContext(environment, services.GetService<DbContextProvider>());
                
                environment.InjectGlobalProperty(jobsManager, "РегламентныеЗадания", true);
                environment.InjectGlobalProperty(jobsManager, "ScheduledJobs", true);

                var runtime = services.GetService<IApplicationRuntime>();
                
                var bgJobsManager = new BackgroundTasksManager(runtime.Engine);
                environment.InjectGlobalProperty(bgJobsManager, "ФоновыеЗадания", true);
                environment.InjectGlobalProperty(bgJobsManager, "BackgroundJobs", true);
            }
        }
    }

    class OscriptBackgroundJobsOptions
    {
        public SupportedJobsStorage StorageType { get; set; }
        
    }

    enum SupportedJobsStorage
    {
        Memory,
        Database
    }

}
