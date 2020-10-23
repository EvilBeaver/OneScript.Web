/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
#if NETCOREAPP
using System.Text;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OneScript.WebHost.Application;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OneScript.DebugServices;
using OneScript.WebHost.Authorization;
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
                new PhysicalFileProvider(svc.GetService<IWebHostEnvironment>().ContentRootPath));
            services.AddTransient<IControllerActivator, ScriptedControllerActivator>();

            InitializeScriptedLayer(services);
            InitializeViewComponents(services);
            InitializeAuthorization(services);

#if NETCOREAPP
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }

        public static void AddOneScriptDebug(this IServiceCollection services, IConfiguration config)
        {
            var debugPort = config.GetValue<int>("debug.port");
            if (debugPort != default)
            {
                var opts = new OscriptDebugOptions();
                opts.DebugPort = debugPort;
                opts.WaitOnStart = config.GetValue<int>("debug.wait") > 0;

                services.AddTransient(sp => Options.Create(opts));
                
                var debugInfrastructure = new BinaryTcpDebugServer(debugPort);
                services.AddSingleton(sp => debugInfrastructure.CreateDebugController());
            }
        }

        private static void InitializeAuthorization(IServiceCollection services)
        {
            services.AddCustomAuthorization();
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

    public class OscriptDebugOptions
    {
        public int DebugPort { get; set; }
        
        public bool WaitOnStart { get; set; }
    }
}
