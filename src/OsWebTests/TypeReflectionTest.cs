using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptEngine.Environment;
using OneScript.WebHost.Infrastructure;
using OneScript.WebHost.Infrastructure.Implementations;

namespace OsWebTests
{
    [TestClass]
    public class TypeReflectionTest
    {
        private LoadedModuleHandle CreateModule(string source)
        {
            var srcProvider = new FakeScriptsProvider();
            srcProvider.Add("/somedir/dummy.os", source);
            var factory = new OneScriptModuleFactory(srcProvider);
            return factory.PrepareModule(srcProvider.Get("/somedir/dummy.os"));
        }

        [TestMethod]
        public void ReflectExportedVariablesAsPublicProperties()
        {
            var code = "Перем А; Перем Б Экспорт;";
            var module = CreateModule(code);
            
            var r = new TypeReflectionEngine();
            Type type = r.Reflect(module, "MyType");

            var props = type.GetProperties(System.Reflection.BindingFlags.Public);
            Assert.AreEqual(type.Name, "MyType");
            Assert.AreEqual(1, props.Length);
            Assert.AreEqual("Б", props[0].Name);
        }
    }
}
