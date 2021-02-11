/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;
using ScriptEngine.Compiler;

namespace OneScript.WebHost.Infrastructure
{
    public class WebCompilerFactory : ICompilerServiceFactory
    {
        private readonly CompilerOptions _options;

        public WebCompilerFactory(CompilerOptions options)
        {
            _options = options;
        }
        
        public ICompilerService CreateInstance(ICompilerContext context)
        {
            // TODO после переопределить
            return new AstBasedCompilerService(_options, context);
        }
    }
}