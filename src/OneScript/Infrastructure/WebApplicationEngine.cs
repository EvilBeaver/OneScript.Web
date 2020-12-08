/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Reflection;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;

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
            Engine.Initialize();
        }
        
        public ScriptingEngine Engine { get; }
        public RuntimeEnvironment Environment { get; }
        public CompilerService GetCompilerService()
        {
            var compilerSvc = Engine.GetCompilerService();
            compilerSvc.DefinePreprocessorValue("ВебСервер");
            compilerSvc.DefinePreprocessorValue("WebServer");

            return compilerSvc;
        }
    }
}
