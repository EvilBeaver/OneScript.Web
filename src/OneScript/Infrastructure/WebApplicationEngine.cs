using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using OneScript.WebHost.Application;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    public class RuntimeOptions
    {
        public bool TracePerAction { get; set; }
        public string TraceDir { get; set; }
    }
    
    public class WebApplicationEngine : IApplicationRuntime
    {
        public WebApplicationEngine():this(null)
        { }

        public WebApplicationEngine(IOptions<RuntimeOptions> config)
        {
            Engine = new ScriptingEngine();
            Environment = new RuntimeEnvironment();
            Engine.Environment = Environment;

            if (!string.IsNullOrEmpty(config.Value?.TraceDir))
            {
                CodeStatCollector = new CodeStatProcessor();
                Engine.SetCodeStatisticsCollector(CodeStatCollector);
            }

            Engine.AttachAssembly(Assembly.GetExecutingAssembly(), Environment);
            Engine.AttachAssembly(typeof(SystemGlobalContext).Assembly, Environment);
            Engine.Initialize();
        }

        // костыль
        public CodeStatProcessor CodeStatCollector { get; }
        public ScriptingEngine Engine { get; }
        public RuntimeEnvironment Environment { get; }


    }
}
