using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;

namespace OneScript.WebHost
{
    public class Startup
    {
        public Startup(IHostingEnvironment hostingEnv, ILoggerFactory logs)
        {
            hostingEnv.ContentRootPath = Path.Combine(hostingEnv.ContentRootPath, "resources");
            hostingEnv.WebRootPath = Path.Combine(hostingEnv.ContentRootPath, "wwwroot");
            logs.AddConsole();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new OscriptViewsOverride());
            });
            services.AddMvc();
            services.AddOneScript();
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
