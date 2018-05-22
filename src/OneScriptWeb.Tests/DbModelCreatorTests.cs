using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using OneScript.WebHost.Database;
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
            var context = new ApplicationDbContext(opts.UseInMemoryDatabase("test1").Options, Mock.Of<IApplicationRuntime>());
        }
    }
}
