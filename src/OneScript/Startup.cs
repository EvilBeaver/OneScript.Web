using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneScript.WebHost.Application;
using OneScript.WebHost.Identity;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;
using OneScript.WebHost.Database;
using OneScript.WebHost.BackgroundJobs;
using OneScript.WebHost.Infobase;
using ScriptEngine;

namespace OneScript.WebHost
{
    public class Startup
    {
        public Startup(IConfiguration conf)
        {
            Configuration = conf;
        }
        
        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new OscriptViewsOverride());
            });
            
            services.AddMemoryCache();
            services.AddSession();
            services.AddDatabaseByConfiguration(Configuration);
            services.AddIdentityByConfiguration(Configuration);
            services.AddBackgroundJobsByConfiguration(Configuration);
            
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                
            });

            services.Configure<GzipCompressionProviderOptions>(options => 
            {
                options.Level = CompressionLevel.Fastest;
            });


            //https://stackoverflow.com/questions/40511103/using-the-antiforgery-cookie-in-asp-net-core-but-with-a-non-default-cookiename
            //TODO добавить sha256 идентификатор приложения генерируемый в момент сборки - чтобы не было пересечений
            //TODO подумать как вывести данную конструкцию в конфигурацю доступную для разработчика 1С
            services.AddAntiforgery(options => options.Cookie.Name = "OScriptWeb.Antiforgery");
            
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
            else
            {
                app.UseStatusCodePages();
            }

            PrepareEnvironment(services);

            var oscriptApp = services.GetService<ApplicationInstance>();
            oscriptApp.OnStartup(app);
            
            // анализ имеющихся компонентов представлений
            var manager = services.GetService<ApplicationPartManager>();
            var provider = manager.FeatureProviders.OfType<ScriptedViewComponentFeatureProvider>().FirstOrDefault();
            provider?.Configure(services);
        }

        private void PrepareEnvironment(IServiceProvider services)
        {
            var environment = services.GetRequiredService<IApplicationRuntime>().Environment;
            DatabaseExtensions.PrepareDbEnvironment(services, environment);
            BackgroundJobsExtensions.PrepareBgJobsEnvironment(services, environment);
            InfobaseExtensions.PrepareIbEnvironment(services, environment);
        }
        
    }
}
