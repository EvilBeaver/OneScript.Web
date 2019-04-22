/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure
{
    public class DynamicCompilationInfo
    {
        public LoadedModule Module { get; set; }
        
        public Type Type { get; set; }
        
        public ICodeSource CodeSource { get; set; }
        
        public object Tag { get; set; }
        
        public DateTime TimeStamp { get; set; }
    }
}