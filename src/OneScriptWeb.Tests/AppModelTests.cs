using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OneScript.WebHost.Infrastructure.Implementations;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class AppModelTests
    {
        [Fact]
        public void CreateAppModelFromSDOTest()
        {
            string testControllerSrc = "Процедура Метод1() КонецПроцедуры";

            var scriptsProvider = new FakeScriptsProvider();
            scriptsProvider.Add("/controllers/mycontroller.os", testControllerSrc);

            var factory = new OneScriptModuleFactory(scriptsProvider);
            var provider = new OscriptApplicationModelProvider(factory);

            var types = new TypeInfo[0];
            var resultContainer = new ApplicationModelProviderContext(types);

            provider.OnProvidersExecuting(resultContainer);

            var result = resultContainer.Result;

            Assert.Equal(1, result.Controllers.Count);
            Assert.Equal("mycontroller", result.Controllers[0].ControllerType.Name);
            Assert.Equal("mycontroller", result.Controllers[0].ControllerName);

            Assert.Equal(1, result.Controllers[0].Actions.Count);
            Assert.Equal("Метод1", result.Controllers[0].Actions[0].ActionName);

        }
    }
}
