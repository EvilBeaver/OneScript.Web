/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OneScript.WebHost.Application;
using ScriptEngine.Machine;
using Microsoft.Extensions.FileProviders;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class ScriptedMiddlewareActivator
    {
        private readonly RequestDelegate _next;
        private readonly LoadedModule _module;
        private readonly IApplicationRuntime _runtime;

        public ScriptedMiddlewareActivator(
            RequestDelegate next, 
            IFileProvider scripts,
            IApplicationRuntime runtime,
            string scriptName)
        {
            _next = next;
            _runtime = runtime;
            var codeSrc = new FileInfoCodeSource(scripts.GetFileInfo(scriptName));
            var image = ScriptedMiddleware.CompileModule(_runtime.GetCompilerService(), codeSrc);
            _module = _runtime.Engine.LoadModuleImage(image);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var instance = new ScriptedMiddleware(_next, _module, _runtime);
            await instance.InvokeAsync(context);
        }
    }
}
