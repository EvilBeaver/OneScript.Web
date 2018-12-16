using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScriptEngine;

namespace OneScript.WebHost.Database
{
    public static class DatabaseExtensions
    {
        public const string ConfigSectionName = "Database";

        public static void AddDatabaseByConfiguration(this IServiceCollection services, IConfiguration config)
        {
            if (!config.GetChildren().Any(item => item.Key == ConfigSectionName))
                return;

            var dbSettings = config.GetSection(ConfigSectionName);

            // Делаем доступным для прочих частей приложения
            services.Configure<OscriptDbOptions>(dbSettings);
            
            AddDatabaseOptions(services);
        }

        private static void AddDatabaseOptions(IServiceCollection services)
        {
            services.AddTransient<DbContextOptions<ApplicationDbContext>>(ConfigureDbOptions);
            services.AddDbContext<ApplicationDbContext>();
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
                default:
                    throw new InvalidOperationException("Unknown database type in configuration");
            }

            return builder.Options;
        }

        internal static InfobaseContext Infobase { get; set; }

        public static void PrepareDbEnvironment(IServiceProvider services, RuntimeEnvironment environment)
        {
            var dbOptions = services.GetService<IOptions<OscriptDbOptions>>().Value;
            if (dbOptions != null && dbOptions.DbType != SupportedDatabase.Unknown)
            {
                var dbctx = services.GetService<ApplicationDbContext>();
                dbctx.Database.EnsureCreated();

                var userManager = new InfobaseUsersManagerContext(services);
                environment.InjectGlobalProperty(userManager, "ПользователиИнформационнойБазы", true);
                environment.InjectGlobalProperty(userManager, "InfoBaseUsers", true);

                var ib = new InfobaseContext();
                Infobase = ib; // Костыль
                ib.DbContext = services.GetRequiredService<ApplicationDbContext>();
                if (dbOptions.DbType == SupportedDatabase.SQLite && dbOptions.ConnectionString == "Data Source=:memory:")
                {
                    var connection = ib.DbContext.Database.GetDbConnection();
                    connection.Open();
                }
                environment.InjectGlobalProperty(ib, "ИнформационнаяБаза", true);
                environment.InjectGlobalProperty(ib, "InfoBase", true);
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
        SQLite
    }

}
