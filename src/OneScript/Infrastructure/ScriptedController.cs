using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    [NonController]
    public class ScriptedController : ScriptDrivenObject
    {
        private ControllerContext _ctx;
        private LoadedModuleHandle _mod;

        public void VoidAction(params object[] parameters)
        {
            var meth = (MethodInfo)_ctx.ActionDescriptor.Properties["actionMethod"];
            if(parameters == null)
                parameters = new object[0];
            meth.Invoke(this, parameters);
        }

        public ScriptedController(ControllerContext context, LoadedModuleHandle module) : base(module)
        {
            _ctx = context;
            _mod = module;
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
