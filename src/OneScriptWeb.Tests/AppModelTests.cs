using System.Reflection;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Reflection;
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

                var scriptsProvider = new InMemoryFileProvider();
                scriptsProvider.AddFile("main.os", "");
                scriptsProvider.AddFile("controllers/mycontroller.os", testControllerSrc);

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

                var scriptsProvider = new InMemoryFileProvider();
                scriptsProvider.AddFile("main.os", "");
                scriptsProvider.AddFile("controllers/mycontroller.os", testControllerSrc);

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

            var scriptsProvider = new InMemoryFileProvider();
            scriptsProvider.AddFile("main.os", "");
            scriptsProvider.AddFile("controllers/mycontroller.os", testControllerSrc);

            var result = CreateApplicationModel(scriptsProvider);

            Assert.Equal(1, result.Controllers.Count);
            Assert.Equal("ScriptedController", result.Controllers[0].ControllerType.Name);
            Assert.Equal("mycontroller", result.Controllers[0].ControllerName);

            Assert.Equal(1, result.Controllers[0].Actions.Count);
            Assert.Equal("Метод1", result.Controllers[0].Actions[0].ActionName);

        }

        [Fact]
        public void TestClassWideAnnotations()
        {
            string testControllerSrc = "#Авторизовать\n" +
                                       "Процедура Метод1() Экспорт КонецПроцедуры";

            var scriptsProvider = new InMemoryFileProvider();
            scriptsProvider.AddFile("main.os", "");
            scriptsProvider.AddFile("controllers/mycontroller.os", testControllerSrc);

            var result = CreateApplicationModel(scriptsProvider);

            Assert.Equal(1, result.Controllers.Count);
            Assert.Equal("ScriptedController", result.Controllers[0].ControllerType.Name);
            Assert.Equal("mycontroller", result.Controllers[0].ControllerName);

            Assert.IsType<AuthorizeAttribute>(result.Controllers[0].Attributes[0]);
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

        private static ApplicationModel CreateApplicationModel(IFileProvider scriptsProvider)
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IFileProvider>(scriptsProvider);
            services.TryAddSingleton(Mock.Of<IConfiguration>());
            services.TryAddSingleton(Mock.Of<ILogger<ApplicationInstance>>());
            services.TryAddSingleton(Mock.Of<IAuthorizationPolicyProvider>());
            services.TryAddScoped<IHostingEnvironment>(x=>new HostingEnvironment()
            {
                ContentRootPath = "/"
            });
            
            services.AddSingleton(CreateWebEngineMock());
            services.AddOneScript();

            var serviceProvider = services.BuildServiceProvider();
            var engine = serviceProvider.GetService<IApplicationRuntime>().Engine;
            engine.DirectiveResolver = new DirectiveMultiResolver();
            var modelProvider = serviceProvider.GetService<IApplicationModelProvider>();

            var types = new TypeInfo[0];
            var resultContainer = new ApplicationModelProviderContext(types);

            modelProvider.OnProvidersExecuting(resultContainer);

            var result = resultContainer.Result;
            return result;
        }
    }
}
