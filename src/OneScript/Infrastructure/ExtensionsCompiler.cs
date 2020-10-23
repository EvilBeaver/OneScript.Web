/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
#if NET461
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OneScript.WebHost.Infrastructure
{
    public class ExtensionsCompiler
    {
        private List<string> _modules = new List<string>();

        public ExtensionsCompiler()
        {
            Dependencies = new List<string>();
        }

        public CompilerResults CompileAssemblyFromStrings()
        {
            var codeProvider = CodeDomProvider.CreateProvider("CSharp");
            var parameters = PrepareParameters();

            return codeProvider.CompileAssemblyFromSource(parameters, _modules.ToArray());
        }

        public void LookupDependencies(string folder)
        {
            var libraries = Directory.EnumerateFiles(folder, "*.dll");
            foreach (var libPath in libraries)
            {
                Dependencies.Add(libPath);
            }
        }

        public CompilerResults CompileAssemblyFromFiles()
        {
            var codeProvider = CodeDomProvider.CreateProvider("CSharp");
            var parameters   = PrepareParameters();

            return codeProvider.CompileAssemblyFromFile(parameters, _modules.ToArray());
        }

        public IList<string> Dependencies { get; }

        private CompilerParameters PrepareParameters()
        {
            var parameters = new CompilerParameters();
            foreach (var libPath in Dependencies)
            {
                parameters.ReferencedAssemblies.Add(libPath);
            }

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Runtime.dll");

            parameters.GenerateInMemory = true;
            parameters.TreatWarningsAsErrors = false;
            parameters.GenerateExecutable = false;
            parameters.CompilerOptions = "/optimize /target:library";

            return parameters;
        }

        public void AddSource(string code)
        {
            _modules.Add(code);
        }
    }
}
#endif
