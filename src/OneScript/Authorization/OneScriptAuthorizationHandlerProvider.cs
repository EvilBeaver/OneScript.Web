using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;

namespace OneScript.WebHost.Authorization
{
    public class OneScriptAuthorizationHandlerProvider : IAuthorizationHandlerProvider
    {
        private readonly List<IAuthorizationHandler> _handlers;

        public OneScriptAuthorizationHandlerProvider(IEnumerable<IAuthorizationHandler> defaultHandlers, IFileProvider filesystem)
        {
            _handlers = new List<IAuthorizationHandler>(defaultHandlers);
            //_handlers.Add(ScriptedAuthorizationHandler);
        }

        public Task<IEnumerable<IAuthorizationHandler>> GetHandlersAsync(AuthorizationHandlerContext context)
        {
            return Task.FromResult<IEnumerable<IAuthorizationHandler>>(_handlers);
        }
    }
}
