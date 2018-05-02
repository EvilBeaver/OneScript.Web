using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;

namespace OneScript.WebHost
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, ILoggerFactory logs)
        {
            if(env.IsDevelopment())
                logs.AddConsole();

            var confBuilder = new ConfigurationBuilder();
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            confBuilder.AddJsonFile(Path.Combine(location, "appsettings.json"), optional:true);
            confBuilder.SetBasePath(Directory.GetCurrentDirectory());
            confBuilder.AddJsonFile("appsettings.json", optional: true);

            Configuration = confBuilder.Build();
            logs.AddConsole(Configuration.GetSection("Logging"));
        }
        
        private IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigurationRoot>(Configuration);

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new OscriptViewsOverride());
            });
            
            services.AddMemoryCache();
            services.AddSession();
            services.AddMvc()
                .ConfigureApplicationPartManager(pm=>pm.FeatureProviders.Add(new ScriptedViewComponentFeatureProvider()));

            services.AddOneScript();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var oscriptApp = services.GetService<ApplicationInstance>();
            oscriptApp.OnStartup(app);
            
            // анализ имеющихся компонентов представлений
            var manager = services.GetService<ApplicationPartManager>();
            var provider = manager.FeatureProviders.OfType<ScriptedViewComponentFeatureProvider>().FirstOrDefault();
            if (provider != null)
            {
                provider.Application = oscriptApp;
                provider.Framework = services.GetService<IApplicationRuntime>();
                provider.ScriptsProvider = services.GetService<IScriptsProvider>();
            }
        }
    }
}
