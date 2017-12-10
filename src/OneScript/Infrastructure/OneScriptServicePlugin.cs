using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScriptEngine.HostedScript;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OneScript.WebHost.Application;
using Microsoft.AspNetCore.Mvc.Controllers;
using OneScript.WebHost.Infrastructure.Implementations;

namespace OneScript.WebHost.Infrastructure
{
    public static class OneScriptServicePlugin
    {
        public static void AddOneScript(this IServiceCollection services)
        {
            services.AddSingleton<IScriptsProvider, FilesystemScriptsProvider>();
            services.AddSingleton<IApplicationModulesLocator, OneScriptModuleFactory>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, OscriptApplicationModelProvider>());
            
            services.AddTransient<IControllerActivator, ScriptedControllerActivator>();

            InitializeScriptedLayer(services);

        }

        private static void InitializeScriptedLayer(IServiceCollection services)
        {
            var webEng = new WebApplicationEngine();
            services.AddSingleton(typeof(WebApplicationEngine), webEng);
            services.AddTransient<IApplicationFactory, AppStarter>();
        }
    }
}
