using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScriptEngine.HostedScript;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OneScript.WebHost.Application;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.FileProviders;
using OneScript.WebHost.Infrastructure.Implementations;

namespace OneScript.WebHost.Infrastructure
{
    public static class OneScriptServicePlugin
    {
        public static void AddOneScript(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, OscriptApplicationModelProvider>());

            services.TryAddTransient<IFileProvider>(svc =>
                new PhysicalFileProvider(svc.GetService<IHostingEnvironment>().ContentRootPath));
            services.AddTransient<IControllerActivator, ScriptedControllerActivator>();

            InitializeScriptedLayer(services);
            InitializeViewComponents(services);
        }

        private static void InitializeViewComponents(IServiceCollection services)
        {
            services.AddSingleton<IViewComponentInvokerFactory, OscriptViewComponentInvokerFactory>();
            services.AddScoped<IViewComponentInvoker, OscriptViewComponentInvoker>();
            services.AddSingleton<IViewComponentActivator, OscriptViewComponentActivator>();
        }

        private static void InitializeScriptedLayer(IServiceCollection services)
        {
            services.TryAddSingleton<IApplicationRuntime, WebApplicationEngine>();
            services.AddTransient<IApplicationFactory, AppStarter>();
            services.AddSingleton<ApplicationInstance>((sp) => 
            {
                var appFactory = (IApplicationFactory)sp.GetService(typeof(IApplicationFactory));
                return appFactory.CreateApp();
            });
        }
    }
}
