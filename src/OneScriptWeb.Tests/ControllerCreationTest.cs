using System.Reflection;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Moq;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;
using ScriptEngine.Environment;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class ControllerCreationTest
    {
        [Fact]
        public void CheckIfControllerCreatedFromScript()
        {
            var fakefs = new InMemoryFileProvider();
            fakefs.AddFile("controllers/test.os","");
            fakefs.AddFile("main.os","");
            var appEngine = new WebApplicationEngine();
            var app = ApplicationInstance.Create(new FileInfoCodeSource(fakefs.GetFileInfo("main.os")), appEngine);
            var provider = new OscriptApplicationModelProvider(app, appEngine, fakefs, Mock.Of<IAuthorizationPolicyProvider>());

            var context = new ApplicationModelProviderContext(new TypeInfo[0]);
            provider.OnProvidersExecuting(context);

            var cc = new ControllerContext();
            var ad = new ControllerActionDescriptor();
            ad.Properties["type"] = context.Result.Controllers[0].Properties["type"];
            ad.Properties["module"] = context.Result.Controllers[0].Properties["module"];
            cc.ActionDescriptor = ad;
            cc.HttpContext = new DefaultHttpContext();
            cc.HttpContext.Session = null;
                
            var activator = new ScriptedControllerActivator(appEngine, null);
            var controller = (ScriptedController)activator.Create(cc);

            Assert.Equal("test", controller.SystemType.Name);
        }

        [Fact]
        public void CheckIfControllerThisObjectAccessible()
        {
            var fakefs = new InMemoryFileProvider();
            fakefs.AddFile("controllers/test.os", "Процедура Б() А = ЭтотОбъект; КонецПроцедуры");
            fakefs.AddFile("main.os", "");
            var appEngine = new WebApplicationEngine();
            var app = ApplicationInstance.Create(new FileInfoCodeSource(fakefs.GetFileInfo("main.os")), appEngine);
            var provider = new OscriptApplicationModelProvider(app, appEngine, fakefs, Mock.Of<IAuthorizationPolicyProvider>());

            var context = new ApplicationModelProviderContext(new TypeInfo[0]);
            provider.OnProvidersExecuting(context);

            var cc = new ControllerContext();
            var ad = new ControllerActionDescriptor();
            ad.Properties["type"] = context.Result.Controllers[0].Properties["type"];
            ad.Properties["module"] = context.Result.Controllers[0].Properties["module"];
            cc.ActionDescriptor = ad;
            cc.HttpContext = new DefaultHttpContext();
            cc.HttpContext.Session = null;

            var activator = new ScriptedControllerActivator(appEngine, null);
            var controller = (ScriptedController)activator.Create(cc);

            Assert.Equal(controller, controller.GetPropValue(0));
        }
    }
}
