using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    [NonController]
    public class ScriptedController : ScriptDrivenObject
    {
        public ScriptedController(LoadedModuleHandle module) : base(module)
        {
        }

        public ScriptedController(LoadedModuleHandle module, bool deffered) : base(module, deffered)
        {
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
