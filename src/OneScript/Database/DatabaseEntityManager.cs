using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Database
{
    public class DatabaseEntityManager : ScriptDrivenObject
    {
        private Type type;

        public DatabaseEntityManager(Type type)
        {
            this.type = type;
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
