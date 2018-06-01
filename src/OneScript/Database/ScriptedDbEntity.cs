using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Database
{
    public class ScriptedDbEntity : ScriptDrivenObject
    {
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
