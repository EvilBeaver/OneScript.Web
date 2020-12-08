/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

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
            var reqs = context.PendingRequirements.OfType<CustomAuthRequirement>();
            foreach (var currentRequirement in reqs)
            {
                var success = currentRequirement.Handle(context);
                if(success)
                    context.Succeed(currentRequirement);
            }
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
