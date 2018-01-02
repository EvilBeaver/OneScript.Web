using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;

namespace OneScriptWeb.Tests
{
    class MinimalTypeSystemHack
    {
        public MinimalTypeSystemHack()
        {
            var Engine = new ScriptingEngine();
            var Environment = new RuntimeEnvironment();
            Engine.Environment = Environment;

            Engine.AttachAssembly(typeof(WebApplicationEngine).Assembly, Environment);
            Engine.AttachAssembly(typeof(SystemGlobalContext).Assembly, Environment);
        }
    }
}
