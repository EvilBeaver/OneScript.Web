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
            services.AddSingleton<IDependencyResolver, FileSystemDependencyResolver>(MakeDependencyResolver);
            
            services.AddTransient<IAstBuilder, DefaultAstBuilder>();
            services.AddTransient<IEngineBuilder, DiEngineBuilder>();
            services.AddSingleton<ITypeManager, DefaultTypeManager>();
            
            // пока не избавились от глобального статического инстанса
            services.AddSingleton<IGlobalsManager>(sp =>
            {
                var instance = new GlobalInstancesManager();
                GlobalsManager.Instance = instance; // вынужденно чистим глобальный инстанс
                return instance;
            });
            services.TryAddSingleton<IApplicationRuntime, WebApplicationEngine>();
            services.AddTransient<IApplicationFactory, AppStarter>();

            services.AddTransient(sp =>
            {
                var opts = new CompilerOptions()
                {
                    NodeBuilder = sp.GetRequiredService<IAstBuilder>()
                };

                opts.UseConditionalCompilation()
                    .UseRegions()
                    .UseImports(sp.GetRequiredService<IDependencyResolver>())
                    .UseDirectiveHandler(o => new ClassAttributeResolver(o.NodeBuilder, o.ErrorSink));

                return opts;
            });
            
            services.AddSingleton((sp) => 
            {
                var appFactory = (IApplicationFactory)sp.GetService(typeof(IApplicationFactory));
                return appFactory.CreateApp();
            });

            services.AddSingleton(sp =>
            {
                var builder = sp.GetRequiredService<IEngineBuilder>();
                var debugger = sp.GetService<IDebugController>();
                if (debugger != default)
                {
                    builder.WithDebugger(debugger);
                }

                var dependencies = sp.GetService<IDependencyResolver>();
                var engine = builder.Build();
                dependencies.Initialize(engine);

                return engine;
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
