using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Authorization
{
    public class CustomAuthRequirement : IAuthorizationRequirement
    {
        private readonly ScriptDrivenObject _provider;

        public CustomAuthRequirement(ScriptDrivenObject provider)
        {
            _provider = provider;
        }

        public bool Handle(AuthorizationHandlerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
