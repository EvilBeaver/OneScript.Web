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
using Microsoft.Extensions.FileProviders;
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
                new PhysicalFileProvider(svc.GetService<IHostingEnvironment>().ContentRootPath));
            services.AddTransient<IControllerActivator, ScriptedControllerActivator>();

            InitializeScriptedLayer(services);
            InitializeViewComponents(services);
            InitializeAuthorization(services);

#if NETCOREAPP
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
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
}
