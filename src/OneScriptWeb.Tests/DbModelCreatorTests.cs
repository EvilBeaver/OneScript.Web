using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OneScript.WebHost.Database;
using OneScript.WebHost.Identity;
using OneScript.WebHost.Infrastructure;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class DbModelCreatorTests
    {
        [Fact]
        public void CanCreateDatabaseInMem()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>();
            var context = new ApplicationDbContext(opts.UseInMemoryDatabase("test1").Options);
        }

        [Fact]
        public void CanReadSecurityOptionsFromConfig()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>();

            var services = new ServiceCollection();
            services.AddTransient<DbContextOptions<ApplicationDbContext>>((svc)=> opts.UseInMemoryDatabase("usersTest").Options);

            var cfgBuilder = new ConfigurationBuilder();
            Dictionary<string,string> keys = new Dictionary<string, string>();
            keys["Security:Password:RequireDigit"] = "false";
            keys["Security:Password:RequireUppercase"] = "false";
            cfgBuilder.AddInMemoryCollection(keys);

            services.AddIdentityByConfiguration(cfgBuilder.Build());

            var provider = services.BuildServiceProvider();
            var result = provider.GetRequiredService<IOptions<IdentityOptions>>().Value;
            Assert.False(result.Password.RequireDigit);
            Assert.False(result.Password.RequireUppercase);
        }

        [Fact]
        public void CanReadDatabaseOptionsFromConfig()
        {
            var services = new ServiceCollection();
            var cfgBuilder = new ConfigurationBuilder();
            Dictionary<string, string> keys = new Dictionary<string, string>();
            keys["Database:DbType"] = "MSSQLServer";
            keys["Database:ConnectionString"] = "blablabla";
            cfgBuilder.AddInMemoryCollection(keys);

            services.AddDatabaseByConfiguration(cfgBuilder.Build());

            var provider = services.BuildServiceProvider();
            var result = provider.GetRequiredService<ApplicationDbContext>();
        }

        [Fact]
        public void CanCreateInfobaseUsers()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>();
            var services = new ServiceCollection();
            services.AddTransient<DbContextOptions<ApplicationDbContext>>((svc) => opts.UseInMemoryDatabase("usersManagerTest").Options);

            var cfgBuilder = new ConfigurationBuilder();
            Dictionary<string, string> keys = new Dictionary<string, string>();
            keys["Security:Password:RequireDigit"] = "false";
            keys["Security:Password:RequireUppercase"] = "false";
            cfgBuilder.AddInMemoryCollection(keys);

            services.AddIdentityByConfiguration(cfgBuilder.Build());
            services.AddDbContext<ApplicationDbContext>();

            var provider = services.BuildServiceProvider();
            var ibUsers = new InfobaseUsersManagerContext(provider);

            var user = ibUsers.CreateUser();
            user.Name = "Hello";
            user.Write();

            var usersArr = ibUsers.GetUsers();
            Assert.Equal(1, usersArr.Count());
            Assert.Equal("Hello", usersArr.Select(x=>(InfobaseUserContext)x).First().Name);
        }
    }
}
