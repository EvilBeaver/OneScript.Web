/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneScript.WebHost.Application;
using OneScript.WebHost.Identity;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;
using OneScript.WebHost.Database;
using OneScript.WebHost.BackgroundJobs;
using ScriptEngine.Machine;

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
            services.AddHttpContextAccessor();
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
            
            services.AddControllersWithViews(mvcopts => mvcopts.EnableEndpointRouting = false)
                .AddRazorRuntimeCompilation()
                .ConfigureApplicationPartManager(pm=>pm.FeatureProviders.Add(new ScriptedViewComponentFeatureProvider()));
            services.AddOneScript();
            services.AddOneScriptDebug(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
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

            StartOneScriptApp(app, services);

            // анализ имеющихся компонентов представлений
            var manager = services.GetService<ApplicationPartManager>();
            var provider = manager.FeatureProviders.OfType<ScriptedViewComponentFeatureProvider>().FirstOrDefault();
            provider?.Configure(services);
        }

        private static void StartOneScriptApp(IApplicationBuilder app, IServiceProvider services)
        {
            var appRuntime = services.GetService<IApplicationRuntime>();
            try
            {
                var oscriptApp = services.GetService<ApplicationInstance>();
                
                MachineInstance.Current.PrepareThread(appRuntime.Environment);
                
                appRuntime.Engine.DebugController = services.GetService<IDebugController>();
                if (appRuntime.DebugEnabled())
                {
                    var logger = services.GetService<ILogger<Startup>>();
                    logger.LogInformation("Debug is enabled");
                    appRuntime.Engine.DebugController.Init();
                    var debugOpts = services.GetService<IOptions<OscriptDebugOptions>>().Value;
                    if (debugOpts.WaitOnStart)
                    {
                        appRuntime.Engine.DebugController.AttachToThread();
                        logger.LogInformation("Waiting for debug client");
                        appRuntime.Engine.DebugController.Wait();
                    }
                }

                oscriptApp.OnStartup(app);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                if (appRuntime.DebugEnabled())
                {
                    appRuntime.Engine.DebugController.DetachFromThread();
                }
            }
        }

        private void PrepareEnvironment(IServiceProvider services)
        {
            var environment = services.GetRequiredService<IApplicationRuntime>().Environment;
            DatabaseExtensions.PrepareDbEnvironment(services, environment);
            BackgroundJobsExtensions.PrepareBgJobsEnvironment(services, environment);
        }
        
    }
}
