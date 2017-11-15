using ScriptEngine.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneScript.WebHost.Infrastructure
{
    public interface IApplicationModulesLocator
    {
        IScriptsProvider SourceProvider { get; }
        LoadedModuleHandle PrepareModule(ICodeSource src);
    }
}
