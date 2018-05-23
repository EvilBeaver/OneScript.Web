using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OneScript.WebHost.Database
{
    public static class DatabaseExtensions
    {
        public static void AddDatabaseByConfiguration(this IServiceCollection services, IConfiguration config)
        {
            var dbSettings = config.GetSection("Database");
            if (dbSettings.Value == null)
                return;

            var options = new OscriptDbOptions();
            dbSettings.Bind(options);

            AddDatabaseOptions(services, options);
            services.AddDbContext<ApplicationDbContext>();
        }

        private static void AddDatabaseOptions(IServiceCollection services, OscriptDbOptions options)
        {
            Func<IServiceProvider, DbContextOptions<ApplicationDbContext>> optionsFactory;
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            switch (options.DbType)
            {
                case SupportedDatabase.MSSQLServer:
                    optionsFactory = (provider) => builder.UseSqlServer(options.ConnectionString).Options;
                    break;
                case SupportedDatabase.Postgres:
                    optionsFactory = (provider) => builder.UseNpgsql(options.ConnectionString).Options;
                    break;
                default:
                    throw new InvalidOperationException("Unknown database type in configuration");
            }

            services.AddTransient<DbContextOptions<ApplicationDbContext>>(optionsFactory);
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
        Postgres
    }

}
