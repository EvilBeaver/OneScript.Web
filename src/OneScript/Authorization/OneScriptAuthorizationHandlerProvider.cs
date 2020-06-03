using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Authorization
{
    public class OneScriptAuthorizationHandlerProvider : IAuthorizationHandlerProvider
    {
        private readonly List<IAuthorizationHandler> _handlers;

        public OneScriptAuthorizationHandlerProvider(IEnumerable<IAuthorizationHandler> defaultHandlers, IFileProvider filesystem, IApplicationRuntime runtime)
        {
            _handlers = new List<IAuthorizationHandler>(defaultHandlers);
            
            AppendScriptedHandlers(runtime, filesystem);
        }

        private void AppendScriptedHandlers(IApplicationRuntime runtime, IFileProvider filesystem)
        {
            var authFile = filesystem.GetFileInfo("auth.os");
            if(!authFile.Exists || authFile.IsDirectory)
                return;

            runtime.Environment.LoadMemory(MachineInstance.Current);
            var codeSource = new FileInfoCodeSource(authFile);
            
            runtime.DebugCurrentThread();

            try
            {
                var registrator = AuthorizationModule.CreateInstance(codeSource, runtime, filesystem);
                registrator.OnRegistration(_handlers);
            }
            finally
            {
                runtime.StopDebugCurrentThread();
            }
        }

        public Task<IEnumerable<IAuthorizationHandler>> GetHandlersAsync(AuthorizationHandlerContext context)
        {
            return Task.FromResult<IEnumerable<IAuthorizationHandler>>(_handlers);
        }
    }
}
