/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            services.AddTransient(_ => opts.UseInMemoryDatabase("usersManagerTest").Options);
            services.AddTransient(_ => Mock.Of<ILogger<UserManager<ApplicationUser>>>());
            services.AddTransient(_ => Mock.Of<ILogger<DataProtectorTokenProvider<ApplicationUser>>>());

            var cfgBuilder = new ConfigurationBuilder();
            var keys = new Dictionary<string, string>
            {
                ["Security:Password:RequireDigit"] = "false",
                ["Security:Password:RequireUppercase"] = "false",
                ["Security:Password:RequireLowercase"] = "false"
            };
            cfgBuilder.AddInMemoryCollection(keys);

            services.AddIdentityByConfiguration(cfgBuilder.Build());
            services.AddDbContext<ApplicationDbContext>();

            var provider = services.BuildServiceProvider();
            var accMock = new Mock<IHttpContextAccessor>();
            accMock.SetupGet(x => x.HttpContext).Returns(() =>
            {
                var hc = new Mock<HttpContext>();
                hc.SetupGet(x => x.RequestServices).Returns(provider);
                return hc.Object;
            });
            var ibUsers = new InfobaseUsersManagerContext(accMock.Object);

            var user = ibUsers.CreateUser();
            user.Name = "Hello";
            user.Write();

            var usersArr = ibUsers.GetUsers();
            Assert.Equal(1, usersArr.Count());
            Assert.Equal("Hello", usersArr.Select(x=>(InfobaseUserContext)x).First().Name);
        }
    }
}
