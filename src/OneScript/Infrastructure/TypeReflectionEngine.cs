using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneScript.WebHost.Infrastructure
{
    public class TypeReflectionEngine
    {
        public Type Reflect(LoadedModuleHandle module, string asTypeName)
        {
            var type = ReflectedClassType.ReflectModule(module, asTypeName);

            return type;
        }
    }
}
