/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
#if NETCOREAPP
using System.Text;
#endif
using System;
using System.IO;
using System.Linq;
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
using OneScript.Language.SyntaxAnalysis;
using OneScript.StandardLibrary;
using OneScript.WebHost.Authorization;
using OneScript.WebHost.Infrastructure.Implementations;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;

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
            var builder = new DiEngineBuilder();
                
            builder
                .WithServices(new AspIoCImplementation(services))
                .SetDefaultOptions()
                .SetupConfiguration(providers =>
                {
                    providers.UseSystemConfigFile()
                        .UseEntrypointConfigFile(Path.Combine(Directory.GetCurrentDirectory(), "main.os"));
                })
                .SetupEnvironment(m =>
                {
                    m.AddStandardLibrary()
                        .AddAssembly(typeof(OneScriptServicePlugin).Assembly);
                });

            builder.Services.UseImports();
            builder.Services.AddDirectiveHandler<ClassAttributeResolver>();

            services.AddSingleton<ICompilerServiceFactory, WebCompilerFactory>();
            services.AddSingleton<IDependencyResolver, FileSystemDependencyResolver>(MakeDependencyResolver);
            services.AddSingleton<IEngineBuilder>(sp =>
            {
                builder.Provider = sp;
                var debugger = sp.GetService<IDebugController>();
                if (debugger != default)
                {
                    builder.WithDebugger(debugger);
                }

                return builder;
            });
            
            services.AddSingleton<IApplicationRuntime, WebApplicationEngine>(sp => 
                new WebApplicationEngine(sp.GetRequiredService<IEngineBuilder>().Build()));
            
            services.AddTransient<IApplicationFactory, AppStarter>();
            services.AddSingleton<ApplicationInstance>(sp => 
            {
                var appFactory = (IApplicationFactory)sp.GetService(typeof(IApplicationFactory));
                return appFactory.CreateApp();
            });
        }

        private static FileSystemDependencyResolver MakeDependencyResolver(IServiceProvider sp)
        {
            var config = sp.GetService<IConfiguration>();
            var resolver = new FileSystemDependencyResolver();

            if (config != default)
            {
                var configSection = config?.GetSection("OneScript");
                var libRoot = configSection?["lib.system"];
                if (libRoot != null)
                {
                    var binFolder = Path.GetDirectoryName(typeof(DiEngineBuilder).Assembly.Location);
                    var additionals = configSection.GetSection("lib.additional")?
                        .AsEnumerable()
                        .Where(x => x.Value != null)
                        .Select(x => x.Value.Replace("$appBinary", binFolder))
                        .ToArray();

                    libRoot = libRoot.Replace("$appBinary", binFolder);
                    resolver.SearchDirectories.Add(libRoot);
                    resolver.SearchDirectories.AddRange(additionals);
                }
            }

            return resolver;
        }
    }

    public class OscriptDebugOptions
    {
        public int DebugPort { get; set; }
        
        public bool WaitOnStart { get; set; }
    }
}
