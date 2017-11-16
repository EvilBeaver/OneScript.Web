using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneScript.WebHost.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace OneScript
{
    public class Startup
    {
        public Startup(IHostingEnvironment hostingEnv, ILoggerFactory logs)
        {
            hostingEnv.ContentRootPath = Path.Combine(hostingEnv.ContentRootPath, "resources");
            logs.AddConsole(LogLevel.Trace)
                .AddDebug(LogLevel.Trace);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore();
            services.AddOneScript();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvcWithDefaultRoute();

        }
    }
}
