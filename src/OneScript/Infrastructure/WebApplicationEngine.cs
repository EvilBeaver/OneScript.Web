/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Hosting;

namespace OneScript.WebHost.Infrastructure
{
    public class WebApplicationEngine : IApplicationRuntime
    {
        public WebApplicationEngine(ScriptingEngine engine)
        {
            Engine = engine;
            Engine.Initialize();
        }

        public ScriptingEngine Engine { get; }
        public RuntimeEnvironment Environment => Engine.Environment;
        
        public ICompilerService GetCompilerService()
        {
            var compilerSvc = Engine.GetCompilerService();
            compilerSvc.DefinePreprocessorValue("ВебСервер");
            compilerSvc.DefinePreprocessorValue("WebServer");

            return compilerSvc;
        }
    }
}
