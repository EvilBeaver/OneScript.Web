/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
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
