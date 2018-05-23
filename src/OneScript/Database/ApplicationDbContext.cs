using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OneScript.WebHost.Identity;
using OneScript.WebHost.Infrastructure;

namespace OneScript.WebHost.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IApplicationRuntime oscript):base(options)
        {
            RuntimeFacility = oscript;
        }

        public IApplicationRuntime RuntimeFacility { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
