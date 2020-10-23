/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using OneScript.WebHost.Infrastructure.Implementations;

namespace OneScriptWeb.Tests
{
    public class ApplicationStartupTests
    {
        [Fact]
        public void CheckThatApplicationInstanceIsCreatedOnMain()
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IFileProvider, InMemoryFileProvider>();

            var cfgBuilder = new ConfigurationBuilder();
            var memData = new Dictionary<string, string>(){ {"OneScript:lib.system", "bla"}};
            cfgBuilder.AddInMemoryCollection(memData);
            services.TryAddSingleton<IConfiguration>(cfgBuilder.Build());
            services.TryAddSingleton<IApplicationRuntime,WebApplicationEngine>();
            services.AddSingleton<IWebHostEnvironment>(Mock.Of<IWebHostEnvironment>());
            services.AddSingleton(Mock.Of<ILogger<ApplicationInstance>>());
            services.AddMvcCore();
            services.AddOneScript();

            var provider = services.BuildServiceProvider();
            var fakeFS = (InMemoryFileProvider)provider.GetService<IFileProvider>();
            fakeFS.AddFile("main.os", "");
            
            var appBuilder = provider.GetService<IApplicationFactory>();
            Assert.NotNull(appBuilder);
            Assert.NotNull(appBuilder.CreateApp()); 
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
        public void CheckThatAppMethodsAreCalled()
        {
            var services = MockMvcServices();
            services.AddSingleton(Mock.Of<IConfiguration>());
            services.AddSingleton(Mock.Of<ILogger<ApplicationInstance>>());
            
            var provider = services.BuildServiceProvider();
            var fakeFS = (InMemoryFileProvider)provider.GetService<IFileProvider>();
            fakeFS.AddFile("main.os", "Процедура ПриНачалеРаботыСистемы()\n" +
                                    "    ИспользоватьСтатическиеФайлы();\n" +
                                    "    ИспользоватьСессии();" +
                                    "КонецПроцедуры");

            var webApp = provider.GetService<IApplicationRuntime>();
            var app = ApplicationInstance.Create(new FileInfoCodeSource(fakeFS.GetFileInfo("main.os")), webApp);
            var mvcAppBuilder = new Mock<IApplicationBuilder>();
            mvcAppBuilder.SetupGet(a => a.ApplicationServices).Returns(provider);

            app.OnStartup(mvcAppBuilder.Object);
            mvcAppBuilder.Verify(x=>x.Use(It.IsAny<Func<RequestDelegate,RequestDelegate>>()), Times.Exactly(2));
            
        }

        private static ServiceCollection MockMvcServices()
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IFileProvider, InMemoryFileProvider>();
            services.AddSingleton(Mock.Of<IWebHostEnvironment>());
            services.AddSingleton(Mock.Of<ILoggerFactory>());
            services.AddTransient(typeof(IActionInvokerFactory), (s) => Mock.Of<IActionInvokerFactory>());
            services.AddTransient(typeof(IActionSelector), (s) => Mock.Of<IActionSelector>());
            services.AddTransient(typeof(DiagnosticSource), (s) => Mock.Of<DiagnosticSource>());

            services.TryAddSingleton<IOptions<ApiBehaviorOptions>>(x=>
            {
                var options = new ApiBehaviorOptions();
                options.InvalidModelStateResponseFactory = (a) => null;
                return new OptionsWrapper<ApiBehaviorOptions>(options);
            });

            var roMock = new Mock<IOptions<RouteOptions>>();
            roMock.SetupGet(x => x.Value).Returns(new RouteOptions()
            {
                AppendTrailingSlash = true,
                ConstraintMap = new Dictionary<string, Type>()
            });
            services.AddTransient(typeof(IOptions<RouteOptions>), (s) => roMock.Object);
            var optsMock = new Mock<IOptions<MvcOptions>>();
            optsMock.SetupGet(x => x.Value).Returns(Mock.Of<MvcOptions>());
            services.AddTransient(typeof(IOptions<MvcOptions>), (s) => optsMock.Object);

            services.AddMvcCore();
            services.AddOneScript();
            return services;
        }

        [Fact(Skip = "Skipped for some reasons")]
        public void CheckThatRoutesAreRegisteredInHandler()
        {
            var services = MockMvcServices();
            services.AddSingleton(Mock.Of<IConfiguration>());
            services.AddSingleton(Mock.Of<ILogger<ApplicationInstance>>());

            var provider = services.BuildServiceProvider();
            var fakeFS = (InMemoryFileProvider)provider.GetService<IFileProvider>();
            fakeFS.AddFile("main.os", "Перем ТестМаршруты Экспорт;\n" +
                                    "Процедура ПриНачалеРаботыСистемы()\n" +
                                    "    ИспользоватьМаршруты(\"РегистрацияМаршрутов\");" +
                                    "КонецПроцедуры\n" +
                                    "\n" +
                                    "Процедура РегистрацияМаршрутов(КоллекцияМаршрутов) Экспорт\n" +
                                    "    ТестМаршруты = КоллекцияМаршрутов\n" +
                                    "КонецПроцедуры");

            var webApp = provider.GetService<IApplicationRuntime>();
            var app = ApplicationInstance.Create(new FileInfoCodeSource(fakeFS.GetFileInfo("main.os")), webApp);
            var mvcAppBuilder = new Mock<IApplicationBuilder>();
            mvcAppBuilder.SetupGet(x => x.ApplicationServices).Returns(provider);

            app.OnStartup(mvcAppBuilder.Object);

            int propIndex = app.FindProperty("ТестМаршруты");
            var routeCollection = app.GetPropValue(propIndex);
            Assert.IsType<RoutesCollectionContext>(routeCollection);
        }

        [Fact]
        public void MethodEchoWritesLog()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IFileProvider, InMemoryFileProvider>();
            services.AddSingleton<IApplicationRuntime, WebApplicationEngine>();
            services.AddSingleton<IConfiguration>(Mock.Of<Func<IServiceProvider, IConfiguration>>());
            var fakeFS = new InMemoryFileProvider();
            fakeFS.AddFile("main.os", "Сообщить(\"Я строка лога\")");
            services.AddSingleton<IFileProvider>(fakeFS);
                
            var loggerMock = new Mock<ILogger<ApplicationInstance>>();
            services.TryAddSingleton(loggerMock.Object);
            services.AddTransient<IApplicationFactory, AppStarter>();

            var provider = services.BuildServiceProvider();
            var starter  = provider.GetService<IApplicationFactory>();

            starter.CreateApp();
            loggerMock.Verify(x => 
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    null,
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void ControllerCreationCanBeCancelled()
        {
            // arrange
            var services = MockMvcServices();
            services.AddSingleton<IConfiguration>(Mock.Of<Func<IServiceProvider, IConfiguration>>());
            services.TryAddSingleton(Mock.Of<IAuthorizationPolicyProvider>());
            var loggerMock = new Mock<ILogger<ApplicationInstance>>();
            services.TryAddSingleton(loggerMock.Object);
            var provider = services.BuildServiceProvider();

            var fakeFs = (InMemoryFileProvider)provider.GetService<IFileProvider>();
            fakeFs.AddFile("main.os", "Процедура ПриРегистрацииКонтроллеров(СписокКонтроллеров, СтандартнаяОбработка)\n" +
                                   "    СтандартнаяОбработка = Ложь;\n" +
                                   "КонецПроцедуры");
            fakeFs.AddFile( "controllers/test.os", "");

            var appModel = provider.GetServices<IApplicationModelProvider>().OfType<OscriptApplicationModelProvider>().First();

            //act
            var context = new ApplicationModelProviderContext(new TypeInfo[0]);
            appModel.OnProvidersExecuting(context);

            // assert
            Assert.Empty(context.Result.Controllers);
        }
    }
}
