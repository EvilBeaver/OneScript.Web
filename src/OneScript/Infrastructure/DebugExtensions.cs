/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;

namespace OneScript.WebHost.Infrastructure
{
    public static class DebugExtensions
    {
        public static bool DebugEnabled(this ScriptingEngine engine)
        {
            return engine.DebugController != null;
        }
        
        public static bool DebugEnabled(this IApplicationRuntime runtime)
        {
            return DebugEnabled(runtime.Engine);
        }
    }
}