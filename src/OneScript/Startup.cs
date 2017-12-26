using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;

namespace OneScript.WebHost
{
    public class Startup
    {
        public Startup(ILoggerFactory logs)
        {
            logs.AddConsole();
        }

        private static void SetContentRoot(IHostingEnvironment hostingEnv, string rootDir)
        {
            hostingEnv.ContentRootPath = Path.GetFullPath(rootDir);
            hostingEnv.WebRootPath = Path.Combine(hostingEnv.ContentRootPath, "wwwroot");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var conf = new ConfigurationBuilder();
            conf.AddEnvironmentVariables("OSWEB_");
            var cfg = conf.Build();
            services.AddSingleton(cfg);

            var rootDir = cfg.GetValue<string>("APP_CONTENT", null);

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new OscriptViewsOverride(rootDir));
            });
            services.AddMvc();
            services.AddOneScript();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services, ILogger<Startup> log)
        {
            if (env.IsDevelopment())
            {
                SetContentRoot(env, env.ContentRootPath);
            }
            else
            {
                var cfg = services.GetService<IConfigurationRoot>();
                var rootDir = cfg.GetValue<string>("APP_CONTENT", null);
                log.LogInformation($"Application content: {rootDir?? "null"}");
                SetContentRoot(env, rootDir ?? Directory.GetCurrentDirectory());

                log.LogInformation($"Content root: {env.ContentRootPath}");
                log.LogInformation($"Web root: {env.WebRootPath}");
            }


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
