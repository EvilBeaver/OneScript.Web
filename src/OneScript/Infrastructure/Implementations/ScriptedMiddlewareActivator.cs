using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OneScript.WebHost.Application;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class ScriptedMiddlewareActivator
    {
        private readonly RequestDelegate _next;
        private readonly LoadedModule _module;
        private readonly IApplicationRuntime _runtime;

        public ScriptedMiddlewareActivator(RequestDelegate next, ICodeSource src, IApplicationRuntime runtime)
        {
            _next = next;
            _runtime = runtime;
            var image = MiddlewareInstance.CompileModule(runtime.Engine.GetCompilerService(), src);
            _module = new LoadedModule(image);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var engine = _runtime.Engine;
            var instance = new MiddlewareInstance(_next, _module);
            var machine = MachineInstance.Current;
            engine.Environment.LoadMemory(machine);
            engine.InitializeSDO(instance);
            await instance.InvokeAsync(context);
        }
    }
}
