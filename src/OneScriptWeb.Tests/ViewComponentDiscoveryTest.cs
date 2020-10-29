/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using System.Reflection;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;
using ScriptEngine;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class ViewComponentDiscoveryTest
    {
        private IApplicationRuntime MakeRuntime()
        {
            var rtMock = new Mock<IApplicationRuntime>();
            var engine = new ScriptingEngine {Environment = new RuntimeEnvironment()};
            rtMock.SetupGet(x => x.Engine).Returns(engine);
            rtMock.SetupGet(x => x.Environment).Returns(engine.Environment);
            rtMock.Setup(x => x.GetCompilerService()).Returns(() => engine.GetCompilerService());

            return rtMock.Object;
        }
        
        [Fact]
        public void CanPopulateViewComponentFeature()
        {
            lock (TestOrderingLock.Lock)
            {
                var services = new ServiceCollection();
                var fakefs = new InMemoryFileProvider();
                fakefs.AddFile("viewComponents/test.os", "Функция ОбработкаВызова() КонецФункции");
                services.AddSingleton<IFileProvider>(fakefs);

                var serviceProvider = services.BuildServiceProvider();
                
                var cp = new ScriptedViewComponentFeatureProvider();

                cp.Runtime = MakeRuntime();
                cp.ScriptsProvider = serviceProvider.GetService<IFileProvider>();

                var feature = new ViewComponentFeature();
                cp.PopulateFeature(new ApplicationPart[0], feature);

                Assert.Equal(1, feature.ViewComponents.Count);
                Assert.Equal("testViewComponent", feature.ViewComponents[0].Name);
            }
        }

        [Fact]
        public void CanIgnoreModulesWithoutInvokator()
        {
            lock (TestOrderingLock.Lock)
            {
                var services = new ServiceCollection();
                var fakefs = new InMemoryFileProvider();
                fakefs.AddFile("viewComponents/test.os", "Функция ДругаяНоНеОбработкаВызова() КонецФункции");
                services.AddSingleton<IFileProvider>(fakefs);

                var serviceProvider = services.BuildServiceProvider();

                var cp = new ScriptedViewComponentFeatureProvider();
                cp.Runtime = MakeRuntime();
                cp.ScriptsProvider = serviceProvider.GetService<IFileProvider>();

                var feature = new ViewComponentFeature();
                cp.PopulateFeature(new ApplicationPart[0], feature);

                Assert.Equal(0, feature.ViewComponents.Count);
            }
        }
        
        [Fact]
        public void CanActivateVC_Through_Activator()
        {
            lock (TestOrderingLock.Lock)
            {
                var services = new ServiceCollection();
                var fakefs = new InMemoryFileProvider();
                fakefs.AddFile("viewComponents/test.os", "Функция ОбработкаВызова() КонецФункции");
                services.AddSingleton<IFileProvider>(fakefs);

                var serviceProvider = services.BuildServiceProvider();

                var cp = new ScriptedViewComponentFeatureProvider();
                cp.Runtime = MakeRuntime();
                cp.ScriptsProvider = serviceProvider.GetService<IFileProvider>();

                var feature = new ViewComponentFeature();
                var pm = new ApplicationPartManager();
                pm.ApplicationParts.Add(new AssemblyPart(Assembly.GetExecutingAssembly()));
                pm.FeatureProviders.Add(cp);
                pm.PopulateFeature(feature);

                var descriptorProvider = new DefaultViewComponentDescriptorProvider(pm);
                var activator = new OscriptViewComponentActivator();
                var descriptor = descriptorProvider.GetViewComponents().First();
                var context = new ViewComponentContext();
                context.ViewComponentDescriptor = descriptor;
                var result = activator.Create(context);

                Assert.IsType<ScriptedViewComponent>(result);
            }
        }

        private static IApplicationRuntime CreateWebEngineMock()
        {
            var webAppMoq = new Mock<IApplicationRuntime>();
            var engine = new ScriptingEngine()
            {
                Environment = new RuntimeEnvironment()
            };

            webAppMoq.SetupGet(x => x.Engine).Returns(engine);
            webAppMoq.SetupGet(x => x.Environment).Returns(engine.Environment);
            return webAppMoq.Object;
        }

        [Fact]
        public void CanDiscoverVCThroughAllPipeline()
        {
            lock (TestOrderingLock.Lock)
            {
                var services = new ServiceCollection();
                var fakefs = new InMemoryFileProvider();
                fakefs.AddFile("viewComponents/test.os", "Функция ОбработкаВызова() КонецФункции");
                services.AddSingleton<IFileProvider>(fakefs);
                services.TryAddSingleton(Mock.Of<IConfiguration>());
                services.TryAddSingleton(Mock.Of<ILogger<ApplicationInstance>>());
                services.TryAddSingleton(Mock.Of<IAuthorizationPolicyProvider>());
                services.TryAddScoped<IWebHostEnvironment>(x => Mock.Of<IWebHostEnvironment>());
                
                var webAppMoq = new Mock<IApplicationRuntime>();
                var engine = new ScriptingEngine()
                {
                    Environment = new RuntimeEnvironment()
                };
                
                webAppMoq.SetupGet(x => x.Engine).Returns(engine);
                webAppMoq.SetupGet(x => x.Environment).Returns(engine.Environment);
                webAppMoq.Setup(x => x.GetCompilerService()).Returns(engine.GetCompilerService());
                
                services.AddSingleton(webAppMoq.Object);
                services.AddMvc()
                    .ConfigureApplicationPartManager(pm => pm.FeatureProviders.Add(new ScriptedViewComponentFeatureProvider()));

                services.AddOneScript();
                services.RemoveAll<ApplicationInstance>();
                var provider = services.BuildServiceProvider();
                var partmanager = provider.GetService<ApplicationPartManager>();
                var finder = partmanager.FeatureProviders.OfType<ScriptedViewComponentFeatureProvider>()
                    .First();

                ((ScriptedViewComponentFeatureProvider)finder).Configure(provider);
                var feature = new ViewComponentFeature();
                partmanager.PopulateFeature<ViewComponentFeature>(feature);

                Assert.Equal(1, feature.ViewComponents.Count);
            }
        }
    }
}
