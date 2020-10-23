/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Linq;
using System.Reflection;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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

        [Fact]
        public void TestActionAnnotationHttpMethod()
        {
            string testControllerSrc = "&HttpMethod(\"GET\")\n" +
                                       "Процедура Метод1() Экспорт КонецПроцедуры";

            var scriptsProvider = new InMemoryFileProvider();
            scriptsProvider.AddFile("main.os", "");
            scriptsProvider.AddFile("controllers/mycontroller.os", testControllerSrc);

            var result = CreateApplicationModel(scriptsProvider);

            var attribs = result.Controllers[0].Actions[0].Attributes;
            Assert.True(attribs.Count == 1);
            Assert.IsType<CustomHttpMethodAttribute>(attribs[0]);
            Assert.Equal("GET", ((CustomHttpMethodAttribute)attribs[0]).HttpMethods.First());
        }

        [Fact]
        public void TestMagicHttpMethodFromActionName()
        {
            string testControllerSrc = "&HttpMethod\n" +
                                       "Процедура Метод1_POST() Экспорт КонецПроцедуры";

            var scriptsProvider = new InMemoryFileProvider();
            scriptsProvider.AddFile("main.os", "");
            scriptsProvider.AddFile("controllers/mycontroller.os", testControllerSrc);

            var result = CreateApplicationModel(scriptsProvider);

            Assert.Equal("Метод1", result.Controllers[0].Actions[0].ActionName);
            var attribs = result.Controllers[0].Actions[0].Attributes;
            Assert.True(attribs.Count == 1);
            Assert.IsType<CustomHttpMethodAttribute>(attribs[0]);
            Assert.Equal("POST", ((CustomHttpMethodAttribute)attribs[0]).HttpMethods.First());
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
            webAppMoq.Setup(x => x.GetCompilerService()).Returns(() => engine.GetCompilerService());
            return webAppMoq.Object;
        }

        private static ApplicationModel CreateApplicationModel(IFileProvider scriptsProvider)
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IFileProvider>(scriptsProvider);
            services.TryAddSingleton(Mock.Of<IConfiguration>());
            services.TryAddSingleton(Mock.Of<ILogger<ApplicationInstance>>());
            services.TryAddSingleton(Mock.Of<IAuthorizationPolicyProvider>());
            services.TryAddScoped<IWebHostEnvironment>(x => Mock.Of<IWebHostEnvironment>());

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
