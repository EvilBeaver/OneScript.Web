using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Reflection;

namespace OneScript.WebHost.Infrastructure
{
    public class TypeReflectionEngine
    {
        public Type Reflect<T>(LoadedModule module, string asTypeName) where T : ScriptDrivenObject
        {
            var typeBuilder = new ClassBuilder<T>();
            var type = typeBuilder
                .SetTypeName(asTypeName)
                .SetModule(module)
                .ExportDefaults()
                .Build();

            return type;
        }
    }
}
