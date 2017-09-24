using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneScript.WebHost.Application
{
    public class ApplicationInstance : ScriptDrivenObject
    {
        public ApplicationInstance(LoadedModuleHandle module): base(module)
        {
            
        }

        protected override int GetOwnMethodCount()
        {
            return 0;
        }

        protected override int GetOwnVariableCount()
        {
            return 0;
        }

        protected override void UpdateState()
        {
            
        }

        internal void OnStartup()
        {
            //throw new NotImplementedException();
        }
    }
}
