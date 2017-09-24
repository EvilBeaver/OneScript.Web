using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.WebHost.Infrastructure;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OsWebTests
{
    [TestClass]
    public class AppModelTests
    {
        [TestMethod]
        public void TestAppModelFromSDO()
        {
            string testControllerSrc = "Процедура Метод1() КонецПроцедуры";

            var scriptsProvider = new FakeScriptsProvider();
            scriptsProvider.Add("/controllers/mycontroller.os", testControllerSrc);

            var factory = new OneScriptScriptFactory(scriptsProvider);
            var provider = new OscriptApplicationModelProvider(factory);

            var types = new TypeInfo[0];
            var resultContainer = new ApplicationModelProviderContext(types);

            provider.OnProvidersExecuting(resultContainer);

            var result = resultContainer.Result;

            Assert.AreEqual(1, result.Controllers.Count);
            Assert.AreEqual(1, result.Controllers[0]);
            Assert.AreEqual("mycontroller", result.Controllers[0].ControllerName);

        }
    }
}
