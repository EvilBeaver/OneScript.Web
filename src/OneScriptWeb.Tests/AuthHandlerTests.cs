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
using Xunit;

namespace OneScriptWeb.Tests
{
    public class AuthHandlerTests
    {
        [Fact]
        public void CheckIfCustomHandlerIsRegistered()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IAuthorizationHandlerProvider, OneScriptAuthorizationHandlerProvider>();
            services.AddSingleton<IAuthorizationHandler, ScriptedAuthorizationHandler>();
            var fakeFS = new InMemoryFileProvider();
            fakeFS.AddFile("auth.os","");

            services.AddCustomAuthorization(fakeFS);
            services.AddSingleton<IFileProvider>(fakeFS);

            var provider = services.BuildServiceProvider();

            var handlers = provider.GetService<IAuthorizationHandlerProvider>();
            var context = new AuthorizationHandlerContext(new IAuthorizationRequirement[0], new ClaimsPrincipal(), null);
            var result = handlers.GetHandlersAsync(context).Result;
            Assert.IsType<OneScriptAuthorizationHandlerProvider>(handlers);
            Assert.True(result.Count() == 1);
            
        }
    }
}
