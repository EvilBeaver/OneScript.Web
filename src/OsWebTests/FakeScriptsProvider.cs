using System.Collections.Generic;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.Environment;
using ScriptEngine;
using System.Linq;

namespace OsWebTests
{
    internal class FakeScriptsProvider : IScriptsProvider
    {
        private ICodeSourceFactory _loader;
        private Dictionary<string, ICodeSource> _sources;

        public FakeScriptsProvider()
        {
            _sources = new Dictionary<string, ICodeSource>();
            var e = new ScriptingEngine();
            _loader = e.Loader;
        }

        public ICodeSource GetCodeSource(string content)
        {
            return _loader.FromString(content);
        }

        public void Add(string virtualPath, string content)
        {
            _sources.Add(virtualPath, GetCodeSource(content));
        }

        public IEnumerable<string> EnumerateEntries(string prefix)
        {
            return _sources.Keys.Where(x => x.StartsWith(prefix));
        }

        public ICodeSource Get(string virtualPath)
        {
            return _sources[virtualPath];
        }
    }
}