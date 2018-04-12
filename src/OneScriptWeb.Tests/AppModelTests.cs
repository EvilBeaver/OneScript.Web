using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;
using ScriptEngine;
using ScriptEngine.Machine;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class AppModelTests
    {
        [Fact]
        public void CreateAppModelFromSDOTest()
        {
            lock (TestOrderingLock.Lock)
            {
                string testControllerSrc = "Процедура Метод1() Экспорт КонецПроцедуры";

                var scriptsProvider = new FakeScriptsProvider();
                scriptsProvider.Add("/main.os", "");
                scriptsProvider.Add("/controllers/mycontroller.os", testControllerSrc);

                var result = CreateApplicationModel(scriptsProvider);

                Assert.Equal(1, result.Controllers.Count);
                Assert.Equal("ScriptedController", result.Controllers[0].ControllerType.Name);
                Assert.Equal("mycontroller", result.Controllers[0].ControllerName);

                Assert.Equal(1, result.Controllers[0].Actions.Count);
                Assert.Equal("Метод1", result.Controllers[0].Actions[0].ActionName); 
                Assert.Equal("VoidAction", result.Controllers[0].Actions[0].ActionMethod.Name);
            }

        }

        [Fact]
        public void CheckThatActionsMappedToFunctions()
        {
            lock (TestOrderingLock.Lock)
            {
                string testControllerSrc = "Функция ВозвращающийМетод() Экспорт КонецФункции";

                var scriptsProvider = new FakeScriptsProvider();
                scriptsProvider.Add("/main.os", "");
                scriptsProvider.Add("/controllers/mycontroller.os", testControllerSrc);

                var result = CreateApplicationModel(scriptsProvider);
                
                Assert.Equal(1, result.Controllers[0].Actions.Count);
                Assert.Equal("ВозвращающийМетод", result.Controllers[0].Actions[0].ActionName);
                Assert.Equal("ResultAction", result.Controllers[0].Actions[0].ActionMethod.Name);
            }

        }

        [Fact]
        public void CheckIfOnlyExportedMethods_AreActions()
        {
            string testControllerSrc = "Процедура Метод1() Экспорт КонецПроцедуры\n" +
                                           "Процедура Метод2() КонецПроцедуры";

            var scriptsProvider = new FakeScriptsProvider();
            scriptsProvider.Add("/main.os", "");
            scriptsProvider.Add("/controllers/mycontroller.os", testControllerSrc);

            var result = CreateApplicationModel(scriptsProvider);

            Assert.Equal(1, result.Controllers.Count);
            Assert.Equal("ScriptedController", result.Controllers[0].ControllerType.Name);
            Assert.Equal("mycontroller", result.Controllers[0].ControllerName);

            Assert.Equal(1, result.Controllers[0].Actions.Count);
            Assert.Equal("Метод1", result.Controllers[0].Actions[0].ActionName);

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

        private static ApplicationModel CreateApplicationModel(FakeScriptsProvider scriptsProvider)
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IScriptsProvider>(scriptsProvider);
            services.TryAddSingleton(Mock.Of<IConfigurationRoot>());
            services.TryAddSingleton(Mock.Of<ILogger<ApplicationInstance>>());
            services.TryAddScoped<IHostingEnvironment>(x=>new HostingEnvironment());
            
            services.AddSingleton(CreateWebEngineMock());
            services.AddOneScript();

            var serviceProvider = services.BuildServiceProvider();
            var modelProvider = serviceProvider.GetService<IApplicationModelProvider>();

            var types = new TypeInfo[0];
            var resultContainer = new ApplicationModelProviderContext(types);

            modelProvider.OnProvidersExecuting(resultContainer);

            var result = resultContainer.Result;
            return result;
        }
    }
}
