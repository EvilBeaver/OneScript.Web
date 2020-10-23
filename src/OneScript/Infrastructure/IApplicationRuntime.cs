using ScriptEngine;
using ScriptEngine.Environment;

namespace OneScript.WebHost.Infrastructure
{
    public interface IApplicationRuntime
    {
        ScriptingEngine Engine { get; }
        RuntimeEnvironment Environment { get; }

        CompilerService GetCompilerService();
    }
}