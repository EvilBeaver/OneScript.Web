/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Threading;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class TypeReflectionTest
    {
        private LoadedModule CreateModule(string source)
        {
            var engine = new ScriptingEngine();
            engine.Environment = new RuntimeEnvironment();
            var compiler = engine.GetCompilerService();
            var byteCode = compiler.Compile(engine.Loader.FromString(source));
            return engine.LoadModuleImage(byteCode);
        }
        
        [Fact]
        public void ReflectExportedVariablesAsPublicFields()
        {
            lock (TestOrderingLock.Lock)
            {
                var code = "Перем А; Перем Б Экспорт;";
                var module = CreateModule(code);
                var r = new TypeReflectionEngine();
                Type type = r.Reflect<UserScriptContextInstance>(module, "MyType");

                var props = type.GetFields();
                Assert.Equal("MyType", type.Name);
                Assert.Single(props);
                Assert.Equal("Б", props[0].Name);
                Assert.True(props[0].IsPublic);
            }
        }
    }
}
