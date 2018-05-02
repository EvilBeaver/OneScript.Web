using Microsoft.AspNetCore.Hosting;
using OneScript.WebHost.Application;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure
{
    public interface IApplicationFactory
    {
        ApplicationInstance CreateApp();
    }
}
