/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
#if NETFRAMEWORK

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using OneScript.WebHost;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class DynamicExtensionTest
    {
        //[Fact]
        public void Test_CompiledExtensionCanUseMvcClasses()
        {
            var extensionsCompiler = new ExtensionsCompiler();

            string code = @"
            using Microsoft.Extensions.DependencyInjection;
            namespace OneScript.WebHost
            {
                public static class Extension {
                    public static void ConfigureServices(IServiceCollection services)
                    {
                    }
                }
            }";

            var asm = typeof(ScriptedController).Assembly;
            var depPaths = Path.GetDirectoryName(asm.CodeBase.Replace(Uri.UriSchemeFile + ":///", ""));
            extensionsCompiler.LookupDependencies(depPaths);
            extensionsCompiler.AddSource(code);
            var result = extensionsCompiler.CompileAssemblyFromStrings();

            Assert.False(result.Errors.HasErrors, ErrorMessages(result.Errors));

        }

        private string ErrorMessages(CompilerErrorCollection err)
        {
            var sb = new StringBuilder();
            foreach (CompilerError compilerError in err)
            {
                sb.AppendLine(compilerError.ToString());
            }

            return sb.ToString();
        }

    }
}
#endif
