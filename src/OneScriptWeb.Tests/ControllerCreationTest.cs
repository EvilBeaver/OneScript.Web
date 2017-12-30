using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;
using ScriptEngine;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class ControllerCreationTest
    {
        [Fact]
        public void CheckIfControllerCreatedFromScript()
        {
            lock (TestOrderingLock.Lock)
            {
                var fakefs = new FakeScriptsProvider();
                fakefs.Add("/controllers/test.os","");
                var app = new WebApplicationEngine();
                var provider = new OscriptApplicationModelProvider(app, fakefs);

                var context = new ApplicationModelProviderContext(new TypeInfo[0]);
                provider.OnProvidersExecuting(context);

                var cc = new ControllerContext();
                var ad = new ControllerActionDescriptor();
                ad.Properties["type"] = context.Result.Controllers[0].Properties["type"];
                ad.Properties["module"] = context.Result.Controllers[0].Properties["module"];
                cc.ActionDescriptor = ad;
                cc.HttpContext = new DefaultHttpContext();
                
                var activator = new ScriptedControllerActivator(app);
                var controller = (ScriptedController)activator.Create(cc);

                Assert.Equal("test", controller.SystemType.Name);
            }
        }
    }
}
