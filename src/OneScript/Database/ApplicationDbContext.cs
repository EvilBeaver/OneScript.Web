using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OneScript.WebHost.Infrastructure;

namespace OneScript.WebHost.Database
{
    public class ApplicationDbContext : DbContext
    {
        private IApplicationRuntime _oscript;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IApplicationRuntime oscript)
        {
            _oscript = oscript;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
