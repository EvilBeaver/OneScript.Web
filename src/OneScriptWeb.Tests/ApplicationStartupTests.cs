using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;

namespace OneScriptWeb.Tests
{
    public class ApplicationStartupTests
    {
        [Fact]
        public void CheckThatApplicationInstanceIsCreatedOnMain()
        {
            lock (TestOrderingLock.Lock)
            {
                var services = new ServiceCollection();
                services.TryAddSingleton<IScriptsProvider, FakeScriptsProvider>();

                var webAppMoq = CreateWebEngineMock();
                services.TryAddSingleton<IApplicationRuntime>(webAppMoq);

                services.AddMvcCore();
                services.AddOneScript();

                var provider = services.BuildServiceProvider();
                var fakeFS = (FakeScriptsProvider)provider.GetService<IScriptsProvider>();
                fakeFS.Add("/main.os", "");

                var appBuilder = provider.GetService<IApplicationFactory>();
                Assert.NotNull(appBuilder);
                Assert.NotNull(appBuilder.CreateApp()); 
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
        public void CheckThatAppMethodsAreCalled()
        {
            lock (TestOrderingLock.Lock)
            {
                var services = MockMvcServices();

                var provider = services.BuildServiceProvider();
                var fakeFS = (FakeScriptsProvider)provider.GetService<IScriptsProvider>();
                fakeFS.Add("/main.os", "Процедура ПриНачалеРаботыСистемы()\n" +
                                       "    ИспользоватьСтатическиеФайлы();\n" +
                                       "    ИспользоватьМаршруты();\n" +
                                       "КонецПроцедуры");

                var webApp = provider.GetService<IApplicationRuntime>();
                var app = ApplicationInstance.Create(fakeFS.Get("/main.os"), webApp);
                var mvcAppBuilder = new Mock<IApplicationBuilder>();
                mvcAppBuilder.SetupGet(x => x.ApplicationServices).Returns(provider);

                app.OnStartup(mvcAppBuilder.Object);
                mvcAppBuilder.Verify(x=>x.Use(It.IsAny<Func<RequestDelegate,RequestDelegate>>()), Times.Exactly(2));

            }
        }

        private static ServiceCollection MockMvcServices()
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IScriptsProvider, FakeScriptsProvider>();
            services.AddSingleton(Mock.Of<IHostingEnvironment>());
            services.AddSingleton(Mock.Of<ILoggerFactory>());
            services.AddTransient(typeof(IActionInvokerFactory), (s) => Mock.Of<IActionInvokerFactory>());
            services.AddTransient(typeof(IActionSelector), (s) => Mock.Of<IActionSelector>());
            services.AddTransient(typeof(DiagnosticSource), (s) => Mock.Of<DiagnosticSource>());

            var roMock = new Mock<IOptions<RouteOptions>>();
            roMock.SetupGet(x => x.Value).Returns(new RouteOptions()
            {
                AppendTrailingSlash = true,
                ConstraintMap = new Dictionary<string, Type>()
            });
            services.AddTransient(typeof(IOptions<RouteOptions>), (s) => roMock.Object);

            services.AddMvcCore();
            services.AddOneScript();
            return services;
        }

        [Fact]
        public void CheckThatRoutesAreRegisteredInHandler()
        {
            lock (TestOrderingLock.Lock)
            {
                var services = MockMvcServices();

                var provider = services.BuildServiceProvider();
                var fakeFS = (FakeScriptsProvider)provider.GetService<IScriptsProvider>();
                fakeFS.Add("/main.os", "Перем ТестМаршруты Экспорт;\n" +
                                       "Процедура ПриНачалеРаботыСистемы()\n" +
                                       "    ИспользоватьМаршруты(\"РегистрацияМаршрутов\");" +
                                       "КонецПроцедуры\n" +
                                       "\n" +
                                       "Процедура РегистрацияМаршрутов(КоллекцияМаршрутов) Экспорт\n" +
                                       "    ТестМаршруты = КоллекцияМаршрутов\n" +
                                       "КонецПроцедуры");

                var webApp = provider.GetService<IApplicationRuntime>();
                var app = ApplicationInstance.Create(fakeFS.Get("/main.os"), webApp);
                var mvcAppBuilder = new Mock<IApplicationBuilder>();
                mvcAppBuilder.SetupGet(x => x.ApplicationServices).Returns(provider);

                app.OnStartup(mvcAppBuilder.Object);

                int propIndex = app.FindProperty("ТестМаршруты");
                var routeCollection = app.GetPropValue(propIndex);
                Assert.IsType<RoutesCollectionContext>(routeCollection);
            }
        }


    }
}
