/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Moq;
using OneScript.WebHost.Authorization;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class AuthHandlerTests
    {
        [Fact]
        public void CheckIfCustomHandlerIsRegistered()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IApplicationRuntime>(CreateWebEngineMock());
            services.AddCustomAuthorization();

            var fakeFs = new InMemoryFileProvider();
            fakeFs.AddFile("auth.os", "Процедура ПриРегистрацииОбработчиков(Список)\n" +
                                      "   Список.Добавить(\"customAuth.os\");\n" +
                                      "КонецПроцедуры");
            fakeFs.AddFile("customAuth.os","");

            services.AddSingleton<IFileProvider>(fakeFs);

            var provider = services.BuildServiceProvider();

            var handlers = provider.GetRequiredService<IAuthorizationHandlerProvider>();
            var context = new AuthorizationHandlerContext(new IAuthorizationRequirement[0], new ClaimsPrincipal(), null);
            var result = handlers.GetHandlersAsync(context).Result;
            Assert.IsType<OneScriptAuthorizationHandlerProvider>(handlers);
            Assert.True(result.Count() == 1);
            
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

    }
}
