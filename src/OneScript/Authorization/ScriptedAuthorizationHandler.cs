using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Authorization
{
    public class ScriptedAuthorizationHandler : ScriptDrivenObject, IAuthorizationHandler
    {
        private ScriptedAuthorizationHandler(LoadedModule module) : base(module)
        {
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            return Task.CompletedTask;
        }

        protected override int GetOwnVariableCount()
        {
            return 0;
        }

        protected override int GetOwnMethodCount()
        {
            return 0;
        }

        protected override void UpdateState()
        {
            
        }

        public static ScriptedAuthorizationHandler CreateInstance(FileInfoCodeSource codeSource, IApplicationRuntime runtime)
        {
            var compiler = runtime.Engine.GetCompilerService();
            var moduleImage = compiler.Compile(codeSource);
            var module = runtime.Engine.LoadModuleImage(moduleImage);

            return new ScriptedAuthorizationHandler(module);
        }
    }
}
