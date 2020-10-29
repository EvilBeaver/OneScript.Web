/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Builder;
using OneScript.WebHost.Infrastructure.Implementations;

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
