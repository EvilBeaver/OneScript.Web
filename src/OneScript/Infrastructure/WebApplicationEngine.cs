using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using OneScript.WebHost.Application;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    public class WebApplicationEngine : IApplicationRuntime
    {
        public WebApplicationEngine()
        {
            Engine = new ScriptingEngine();
            Environment = new RuntimeEnvironment();
            Engine.Environment = Environment;

            Engine.AttachAssembly(Assembly.GetExecutingAssembly(), Environment);
            Engine.AttachAssembly(typeof(SystemGlobalContext).Assembly, Environment);
            // TODO Убрать после реализации https://github.com/EvilBeaver/OneScript/issues/641
            TypeManager.RegisterType("Сценарий", typeof(UserScriptContextInstance));
            Engine.Initialize();
        }
        
        public ScriptingEngine Engine { get; }
        public RuntimeEnvironment Environment { get; }


    }
}
