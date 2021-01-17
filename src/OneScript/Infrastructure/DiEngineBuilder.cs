// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure
{
    public class DiEngineBuilder : DefaultEngineBuilder
    {
        public DiEngineBuilder(
            IGlobalsManager globals,
            ITypeManager typeManager,
            CompilerOptions options,
            IWebHostEnvironment env)
        {
            GlobalInstances = globals;
            TypeManager = typeManager;
            CompilerOptions = options;

            this.WithEnvironment(new RuntimeEnvironment())
                .UseEnvironmentVariableConfig("OSCRIPT_CONFIG")
                .UseEntrypointConfigFile(Path.Combine(env.ContentRootPath, "main.os"))
                .UseSystemConfigFile();

            this.AddAssembly(typeof(ArrayImpl).Assembly)
                .AddAssembly(typeof(SystemGlobalContext).Assembly)
                .AddAssembly(typeof(DiEngineBuilder).Assembly);
        }
    }
}