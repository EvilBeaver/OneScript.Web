using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneScript.WebHost.Infrastructure;

namespace OneScript.WebHost
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, ILoggerFactory logs)
        {
            if(env.IsDevelopment())
                logs.AddConsole();

            var confBuilder = new ConfigurationBuilder();
            var location = Directory.GetCurrentDirectory();
            confBuilder.AddJsonFile(Path.Combine(location, "appsettings.json"), optional:true);
            confBuilder.SetBasePath(Directory.GetCurrentDirectory());
            confBuilder.AddJsonFile("appsettings.json", optional: true);

            Configuration = confBuilder.Build();
            logs.AddConsole(Configuration);
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
            services.AddMvc();
            services.AddOneScript();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services, ILogger<Startup> log)
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
