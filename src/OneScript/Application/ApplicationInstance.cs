using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace OneScript.WebHost.Application
{
    public partial class ApplicationInstance : ScriptDrivenObject
    {
        public ApplicationInstance(LoadedModuleHandle module): base(module)
        {
            
        }

        protected override int GetOwnMethodCount()
        {
            return 1;
        }

        protected override int GetOwnVariableCount()
        {
            return 0;
        }

        protected override void UpdateState()
        {
            
        }

        [ContextMethod("ИспользоватьСтатическиеФайлы")]
        public void UseStaticFiles()
        {
            throw new NotImplementedException();
        }

        [ContextMethod("ИспользоватьМаршруты")]
        public void UseMvcRoutes()
        {
            throw new NotImplementedException();
        }

        internal void OnStartup(IApplicationBuilder aspAppBuilder)
        {
            int startup = GetScriptMethod("ПриНачалеРаботыСистемы", "OnSystemStartup");
            if(startup == -1)
                return;

            throw new NotImplementedException();
        }
    }
}
