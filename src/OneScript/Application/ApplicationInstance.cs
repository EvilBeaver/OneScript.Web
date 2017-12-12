using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Application
{
    public class ApplicationInstance : ScriptDrivenObject
    {
        private readonly ContextMethodsMapper<ApplicationInstance> _ownMethods = new ContextMethodsMapper<ApplicationInstance>();

        public ApplicationInstance(LoadedModuleHandle module): base(module)
        {
            
        }

        protected override int GetOwnMethodCount()
        {
            return _ownMethods.Count;
        }

        protected override int GetOwnVariableCount()
        {
            return 0;
        }

        protected override void UpdateState()
        {
            
        }

        protected override int FindOwnMethod(string name)
        {
            return _ownMethods.FindMethod(name);
        }

        protected override MethodInfo GetOwnMethod(int index)
        {
            return _ownMethods.GetMethodInfo(index);
        }

        protected override void CallOwnProcedure(int index, IValue[] arguments)
        {
            _ownMethods.GetMethod(index)(this, arguments);
        }

        protected override IValue CallOwnFunction(int index, IValue[] arguments)
        {
            return _ownMethods.GetMethod(index)(this, arguments);
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
