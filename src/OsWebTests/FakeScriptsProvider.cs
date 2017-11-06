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

        private ICodeSource CreateCodeSource(string virtualPath, string content)
        {
            var src = _loader.FromString(content);
            return new FakeCodeSource(src, virtualPath);
        }

        public void Add(string virtualPath, string content)
        {
            _sources.Add(virtualPath, CreateCodeSource(virtualPath, content));
        }

        public IEnumerable<string> EnumerateFiles(string prefix)
        {
            return _sources.Keys.Where(x => x.StartsWith(prefix));
        }

        public ICodeSource Get(string virtualPath)
        {
            return _sources[virtualPath];
        }
    }

    class FakeCodeSource : ICodeSource
    {
        ICodeSource _original;
        string _name;

        public FakeCodeSource(ICodeSource original, string fakeFileName)
        {
            _original = original;
            _name = fakeFileName;
        }

        public string Code => _original.Code;

        public string SourceDescription => _name;
    }

}