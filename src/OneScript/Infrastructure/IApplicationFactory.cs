using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OneScript.WebHost.Application;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure
{
    public interface IApplicationFactory
    {
        ApplicationInstance CreateApp(IServiceCollection services);
    }
}
