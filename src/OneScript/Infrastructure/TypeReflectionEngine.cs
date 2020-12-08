/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;
using System;
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
