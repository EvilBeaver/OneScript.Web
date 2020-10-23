using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OneScript.WebHost.Application;
using ScriptEngine;
using ScriptEngine.Environment;
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
            var engine = _runtime.Engine;
            var instance = new ScriptedMiddleware(_next, _module);
            var machine = MachineInstance.Current;
            engine.Environment.LoadMemory(machine);
            try
            {
                _runtime.DebugCurrentThread();
                engine.InitializeSDO(instance);
                await instance.InvokeAsync(context);
            }
            finally
            {
                _runtime.StopDebugCurrentThread();
            }
            
        }
    }
}
