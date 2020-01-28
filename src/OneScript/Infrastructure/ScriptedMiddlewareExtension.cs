using Microsoft.AspNetCore.Builder;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure.Implementations;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneScript.WebHost.Infrastructure
{
    /// <summary>
    /// Расширение для создания посредников
    /// </summary>
    public static class ScriptedMiddlewareExtension
    {
        public static IApplicationBuilder UseScriptedMiddleware(
             this IApplicationBuilder app, string scriptName)
        {
            return app.UseMiddleware<ScriptedMiddlewareActivator>(scriptName);
        }

    }
}
