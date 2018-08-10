using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Authorization
{
    public class ScriptedAuthorizationHandler : ScriptDrivenObject, IAuthorizationHandler
    {
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
    }
}
