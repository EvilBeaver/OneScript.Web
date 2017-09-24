using ScriptEngine.Environment;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine.Contexts;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using ScriptEngine;
using System;

namespace OneScript.WebHost.Infrastructure
{
    public class OneScriptScriptFactory
    {
        private ScriptingEngine _eng = new ScriptingEngine();
        private RuntimeEnvironment _globalEnv = new RuntimeEnvironment();

        public OneScriptScriptFactory()
        {
            _eng.Environment = _globalEnv;
        }

        public OneScriptScriptFactory(IScriptsProvider scriptsProvider) : this()
        {
            SourceProvider = scriptsProvider;
        }

        public IScriptsProvider SourceProvider { get; }

        public LoadedModuleHandle PrepareModule(string virtualPath)
        {
            throw new NotImplementedException();

            var src = _eng.Loader.FromFile(virtualPath);
            return PrepareModule(src);
        }

        public LoadedModuleHandle PrepareModule(ICodeSource src)
        {
            var compiler = _eng.GetCompilerService();
            var image = compiler.CreateModule(src);

            return _eng.LoadModuleImage(image);
        }

    }
}
