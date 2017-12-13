using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
                var services = new ServiceCollection();
                services.TryAddSingleton<IScriptsProvider, FakeScriptsProvider>();
                services.AddMvcCore();
                services.AddOneScript();

                var provider = services.BuildServiceProvider();
                var fakeFS = (FakeScriptsProvider)provider.GetService<IScriptsProvider>();
                fakeFS.Add("/main.os", "Процедура ПриНачалеРаботыСистемы()\n" +
                                       "    ИспользоватьСтатическиеФайлы();\n" +
                                       "    ИспользоватьМаршруты();" +
                                       "КонецПроцедуры");

                var webApp = provider.GetService<IApplicationRuntime>();
                var app = ApplicationInstance.Create(fakeFS.Get("/main.os"), webApp);
                var mvcAppBuilder = new Mock<IApplicationBuilder>();
                
                app.OnStartup(mvcAppBuilder.Object);
                mvcAppBuilder.Verify(x=>x.Use(It.IsAny<Func<RequestDelegate,RequestDelegate>>()), Times.Exactly(2));

            }
        }
    }
}
