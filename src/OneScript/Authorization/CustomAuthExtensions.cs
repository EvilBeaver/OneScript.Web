using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace OneScript.WebHost.Authorization
{
    public static class CustomAuthExtensions
    {
        public static void AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandlerProvider, OneScriptAuthorizationHandlerProvider>();
        }
    }
}
