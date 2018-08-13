using System;
using ScriptEngine;

namespace OneScript.WebHost.Infobase
{
    public static class InfobaseExtensions
    {
        public static void PrepareIbEnvironment(IServiceProvider services, RuntimeEnvironment environment)
        {
            var infobase = new InfobaseManagerContext(services);

            environment.InjectGlobalProperty(infobase, "ИнформационнаяБаза", true);
            environment.InjectGlobalProperty(infobase, "InfoBase", true);
        }
    }

}
