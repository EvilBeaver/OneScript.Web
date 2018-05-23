using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneScript.WebHost.Database;

namespace OneScript.WebHost.Identity
{
    public static class IdentityExtensions
    {
        public static void AddIdentityByConfiguration(this IServiceCollection services, IConfiguration config)
        {
            const string keyName = "Security";
            if (!config.GetChildren().Any(item => item.Key == keyName))
                return;

            var security = config.GetSection(keyName);
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    ProcessSecurityOptions(options, security);
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var authBuilder = services.AddAuthentication();
            var cookieOpts = security.GetSection("CookieAuth");
            if (cookieOpts != null)
            {
                authBuilder.AddCookie(options =>
                {
                    options.LoginPath = cookieOpts["LoginPath"] ?? options.LoginPath;
                    options.LogoutPath = cookieOpts["LogoutPath"] ?? options.LogoutPath;
                    options.AccessDeniedPath = cookieOpts["AccessDeniedPath"] ?? options.AccessDeniedPath;
                    options.ExpireTimeSpan = cookieOpts["ExpireTimeSpan"] == null? options.ExpireTimeSpan : TimeSpan.Parse(cookieOpts["ExpireTimeSpan"]);
                    cookieOpts.Bind("Cookie", options.Cookie);
                });
            }

        }

        private static void ProcessSecurityOptions(IdentityOptions options, IConfigurationSection security)
        {
            security.Bind("Password", options.Password);
            security.Bind("User", options.User);
            security.Bind("Lockout", options.Lockout);
        }
    }
}
