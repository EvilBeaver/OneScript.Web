using System;
using System.Threading;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;
using ScriptEngine;
using ScriptEngine.Environment;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class TypeReflectionTest
    {
        private LoadedModuleHandle CreateModule(string source)
        {
            var engine = new ScriptingEngine();
            engine.Environment = new RuntimeEnvironment();
            var compiler = engine.GetCompilerService();
            var byteCode = compiler.CreateModule(engine.Loader.FromString(source));
            return engine.LoadModuleImage(byteCode);
        }
        
        [Fact]
        public void ReflectExportedVariablesAsPublicProperties()
        {
            lock (TestOrderingLock.Lock)
            {
                var code = "Перем А; Перем Б Экспорт;";
                var module = CreateModule(code);
                var r = new TypeReflectionEngine();
                Type type = r.Reflect(module, "MyType");

                var props = type.GetProperties(System.Reflection.BindingFlags.Public);
                Assert.Equal(type.Name, "MyType");
                Assert.Equal(1, props.Length);
                Assert.Equal("Б", props[0].Name);
            }
        }
    }
}
