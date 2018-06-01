using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
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
                cp.Engine = new ScriptingEngine();
                cp.Engine.Environment = new RuntimeEnvironment();
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
                cp.Engine = new ScriptingEngine();
                cp.Engine.Environment = new RuntimeEnvironment();
                cp.ScriptsProvider = serviceProvider.GetService<IFileProvider>();

                var feature = new ViewComponentFeature();
                cp.PopulateFeature(new ApplicationPart[0], feature);

                Assert.Equal(0, feature.ViewComponents.Count);
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
                services.TryAddScoped<IHostingEnvironment>(x => new HostingEnvironment()
                {
                    ContentRootPath = "/"
                });
                
                var webAppMoq = new Mock<IApplicationRuntime>();
                var engine = new ScriptingEngine()
                {
                    Environment = new RuntimeEnvironment()
                };
                
                webAppMoq.SetupGet(x => x.Engine).Returns(engine);
                webAppMoq.SetupGet(x => x.Environment).Returns(engine.Environment);
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
