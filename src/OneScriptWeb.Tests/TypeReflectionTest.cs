using System;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;
using ScriptEngine.Environment;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class TypeReflectionTest
    {
        private LoadedModuleHandle CreateModule(string source)
        {
            var srcProvider = new FakeScriptsProvider();
            srcProvider.Add("/somedir/dummy.os", source);
            var factory = new OneScriptModuleFactory(srcProvider);
            return factory.PrepareModule(srcProvider.Get("/somedir/dummy.os"));
        }

        [Fact]
        public void ReflectExportedVariablesAsPublicProperties()
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
