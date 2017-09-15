using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine.Contexts;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using ScriptEngine;

namespace OneScript.WebHost.Infrastructure
{
    public class OneScriptScriptFactory
    {
        private HostedScriptEngine _hse= new HostedScriptEngine();

        private class DummyStartupSource : ICodeSource
        {
            public DummyStartupSource()
            {
                Code = "startup.os";
                SourceDescription = "startup.os";
            }

            public string Code { get; }
            public string SourceDescription { get; }
        }

        public OneScriptScriptFactory(string applicationRoot)
        {
            ApplicationRoot = applicationRoot;
            _hse.SetGlobalEnvironment(new WebApplicationHost(), new DummyStartupSource());
            _hse.Initialize();
        }

        public string ApplicationRoot { get; }
        
    }
}
