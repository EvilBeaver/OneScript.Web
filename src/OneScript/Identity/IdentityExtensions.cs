/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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

            var cookieOpts = security.GetSection("CookieAuth");
            if (cookieOpts != null)
            {
                services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = cookieOpts["LoginPath"] ?? options.LoginPath;
                    options.LogoutPath = cookieOpts["LogoutPath"] ?? options.LogoutPath;
                    options.AccessDeniedPath = cookieOpts["AccessDeniedPath"] ?? options.AccessDeniedPath;
                    options.ExpireTimeSpan = cookieOpts["ExpireTimeSpan"] == null? options.ExpireTimeSpan : TimeSpan.Parse(cookieOpts["ExpireTimeSpan"]);
                    options.ReturnUrlParameter = cookieOpts["ReturnUrlParameter"] ?? options.ReturnUrlParameter;
                    options.Cookie.Name = cookieOpts["CookieName"] ?? "OscriptWeb.Identity.Application";

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
