using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine.Environment;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class FilesystemScriptsProvider : IScriptsProvider
    {
        private readonly string _contentRoot;
        private readonly ICodeSourceFactory _loader;

        public FilesystemScriptsProvider(IHostingEnvironment env)
        {
            _contentRoot = env.ContentRootPath;
            var e = new ScriptEngine.ScriptingEngine();
            _loader = e.Loader;
        }

        public IEnumerable<string> EnumerateFiles(string prefix)
        {
            return Directory.EnumerateFiles(_contentRoot + prefix, "*.os")
                .Select(x=>x.Substring(_contentRoot.Length));
        }

        public ICodeSource Get(string virtualPath)
        {
            var realPath = Path.Combine(_contentRoot, virtualPath.TrimStart('/'));
            return _loader.FromFile(realPath);
        }
    }
}
