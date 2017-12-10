using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneScript.WebHost.Infrastructure;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OneScript.WebHost.Application;

namespace OneScript
{
    public class Startup
    {
        public Startup(IHostingEnvironment hostingEnv, ILoggerFactory logs)
        {
            hostingEnv.ContentRootPath = Path.Combine(hostingEnv.ContentRootPath, "resources");
            logs.AddConsole();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore();
            services.AddOneScript();
            services.AddSession();

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var appFactory = (IApplicationFactory)services.GetService(typeof(IApplicationFactory));
            var oscriptApp = appFactory.CreateApp();
            oscriptApp.OnStartup(app);
        }
    }
}
