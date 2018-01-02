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

namespace OneScript.WebHost.Infrastructure
{
    public class WebApplicationEngine : IApplicationRuntime
    {
        public WebApplicationEngine()
        {
            Engine = new ScriptingEngine();
            Environment = new RuntimeEnvironment();
            Engine.Environment = Environment;

            Engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly(), Environment);
            Engine.AttachAssembly(typeof(SystemGlobalContext).Assembly, Environment);
            Environment.InjectObject(new WebGlobalContext());
            Engine.Initialize();
            Engine.UpdateContexts();
        }
        
        public ScriptingEngine Engine { get; }
        public RuntimeEnvironment Environment { get; }


    }
}
