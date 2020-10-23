/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
            ad.Properties["CompilationInfo"] = context.Result.Controllers[0].Properties["CompilationInfo"];
            cc.ActionDescriptor = ad;
            cc.HttpContext = new DefaultHttpContext();
            cc.HttpContext.Session = null;
                
            var activator = new ScriptedControllerActivator(appEngine);
            var controller = (ScriptedController)activator.Create(cc);

            Assert.Equal("Контроллер.test", controller.SystemType.Name);
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
            ad.Properties["CompilationInfo"] = context.Result.Controllers[0].Properties["CompilationInfo"];
            cc.ActionDescriptor = ad;
            cc.HttpContext = new DefaultHttpContext();
            cc.HttpContext.Session = null;

            var activator = new ScriptedControllerActivator(appEngine);
            var controller = (ScriptedController)activator.Create(cc);

            Assert.Equal(controller, controller.GetPropValue(0));
        }
    }
}
