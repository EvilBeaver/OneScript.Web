using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Moq;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;

namespace OneScriptWeb.Tests
{
    public class ApplicationStartupTests
    {
        [Fact]
        public void CheckThatApplicationInstanceIsCreatedOnMain()
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IScriptsProvider, FakeScriptsProvider>();
            services.AddMvcCore();
            services.AddOneScript();

            var provider = services.BuildServiceProvider();
            var fakeFS = (FakeScriptsProvider)provider.GetService<IScriptsProvider>();
            fakeFS.Add("/main.os", "");

            var appBuilder = provider.GetService<IApplicationFactory>();
            Assert.NotNull(appBuilder);

            appBuilder.CreateApp();
        }

        [Fact]
        public void CheckThatAppMethodsAreCalled()
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IScriptsProvider, FakeScriptsProvider>();
            services.AddMvcCore();
            services.AddOneScript();
            
            var provider = services.BuildServiceProvider();
            var fakeFS = (FakeScriptsProvider)provider.GetService<IScriptsProvider>();
            fakeFS.Add("/main.os", "Процедура ПриНачалеРаботыСистемы()\n" +
                                   "    ИспользоватьСтатическиеФайлы()" +
                                   "КонецПроцедуры");

            var locator = provider.GetService<IApplicationModulesLocator>();
            var app = new ApplicationInstance(locator.PrepareModule(fakeFS.Get("/main.os")));

            var appMock = Mock.Get(app);
            appMock.Verify(x => x.UseStaticFiles());

            appMock.Object.OnStartup(Mock.Of<IApplicationBuilder>());
        }
    }
}
