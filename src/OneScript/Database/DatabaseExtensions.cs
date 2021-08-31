/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;

namespace OneScript.WebHost.Database
{
    public static class DatabaseExtensions
    {
        public const string ConfigSectionName = "Database";

        public static void AddDatabaseByConfiguration(this IServiceCollection services, IConfiguration config)
        {
            var dbSettings = config.GetSection(ConfigSectionName);
            if (dbSettings.Exists())
            {
                // Делаем доступным для прочих частей приложения
                services.Configure<OscriptDbOptions>(dbSettings);
                AddDatabaseOptions(services);
            }
        }

        private static void AddDatabaseOptions(IServiceCollection services)
        {
            services.AddTransient<DbContextOptions<ApplicationDbContext>>(ConfigureDbOptions);
            services.AddDbContext<ApplicationDbContext>();
            services.AddTransient<DbContextProvider>();
        }

        private static DbContextOptions<ApplicationDbContext> ConfigureDbOptions(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<OscriptDbOptions>>().Value;
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            switch (options.DbType)
            {
                case SupportedDatabase.MSSQLServer:
                    builder.UseSqlServer(options.ConnectionString);
                    break;
                case SupportedDatabase.Postgres:
                    builder.UseNpgsql(options.ConnectionString);
                    break;
                case SupportedDatabase.SQLite:
                    builder.UseSqlite(options.ConnectionString);
                    break;
                case SupportedDatabase.MySQL:
                    builder.UseMySql(options.ConnectionString);
                    break;
                default:
                    throw new InvalidOperationException("Unknown database type in configuration");
            }

            return builder.Options;
        }

        internal static InfobaseContext Infobase { get; set; }

        public static void PrepareDbEnvironment(IServiceProvider services, RuntimeEnvironment environment)
        {
            var logger = services.GetService<ILogger<ApplicationInstance>>();
            var dbOptions = services.GetService<IOptions<OscriptDbOptions>>().Value;
            if (dbOptions != null && dbOptions.DbType != SupportedDatabase.Unknown)
            {
                logger.LogDebug($"Database enabled: {dbOptions.DbType}");
                var dbctx = services.GetService<ApplicationDbContext>();
                dbctx.Database.EnsureCreated();

                var userManager = new InfobaseUsersManagerContext(services.GetRequiredService<IHttpContextAccessor>());
                environment.InjectGlobalProperty(userManager, "ПользователиИнформационнойБазы", true);
                environment.InjectGlobalProperty(userManager, "InfoBaseUsers", true);

                var ib = new InfobaseContext();
                Infobase = ib; // Костыль
                ib.DbContext = services.GetRequiredService<ApplicationDbContext>();
                environment.InjectGlobalProperty(ib, "ИнформационнаяБаза", true);
                environment.InjectGlobalProperty(ib, "InfoBase", true);
            }
            else
            {
                logger.LogDebug("No database configured");
            }
        }
    }

    class OscriptDbOptions
    {
        public SupportedDatabase DbType { get; set; }
        public string ConnectionString { get; set; }
    }

    enum SupportedDatabase
    {
        Unknown,
        MSSQLServer,
        Postgres,
        SQLite,
        MySQL
    }

}
