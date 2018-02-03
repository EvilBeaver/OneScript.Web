using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;

namespace OneScript.WebHost.Infrastructure
{
    // TODO: Сделать публичными в основном движке
    public class LibraryResolverAdHoc : IDirectiveResolver
    {
        private IDirectiveResolver _stdResolver;

        public LibraryResolverAdHoc(IApplicationRuntime runtime, string libRoot, string[] additionals)
        {
            _stdResolver = CaptureStdResolverAdHoc(runtime, libRoot, additionals);
        }

        private IDirectiveResolver CaptureStdResolverAdHoc(IApplicationRuntime runtime, string root, string[] additionals)
        {
            var asm = typeof(SystemGlobalContext).Assembly;
            var resolverType = asm.GetType("ScriptEngine.HostedScript.LibraryResolver", true);
            var args = new object[] {runtime.Engine, runtime.Environment};
            var ctor = resolverType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                args.Select(x=>x.GetType()).ToArray(), null);

            var resolver = ctor.Invoke(args);
            resolverType.GetProperty("LibraryRoot")?.SetValue(resolver, root);

            if (additionals != null)
            {
                resolverType.GetProperty("SearchDirectories")?.SetValue(resolver, additionals.ToList());
            }

            return (IDirectiveResolver)resolver;
        }

        public bool Resolve(string directive, string value, bool codeEntered)
        {
            return _stdResolver.Resolve(directive, value, codeEntered);
        }

        public ICodeSource Source
        {
            get { return _stdResolver.Source; }
            set { _stdResolver.Source = value; }
        }
    }
}