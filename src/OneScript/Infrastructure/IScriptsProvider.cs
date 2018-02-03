using ScriptEngine.Environment;
using System.Collections.Generic;

namespace OneScript.WebHost.Infrastructure
{
    public interface IScriptsProvider
    {
        ICodeSource Get(string virtualPath);
        IEnumerable<string> EnumerateFiles(string prefix);
    }
}