using ScriptEngine.Environment;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine.Contexts;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OneScriptModuleFactory : IApplicationModulesLocator
    {
        private ScriptingEngine _eng = new ScriptingEngine();
        private RuntimeEnvironment _globalEnv = new RuntimeEnvironment();
        
        public OneScriptModuleFactory(IScriptsProvider scriptsProvider)
        {
            _eng.Environment = _globalEnv;
            SourceProvider = scriptsProvider;
        }

        public IScriptsProvider SourceProvider { get; }

        public LoadedModuleHandle PrepareModule(ICodeSource src)
        {
            var compiler = _eng.GetCompilerService();
            var image = compiler.CreateModule(src);

            return _eng.LoadModuleImage(image);
        }

    }
}
