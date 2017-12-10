using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    public class RouteDescriptionContext : AutoContext<RouteDescriptionContext>
    {
        [ContextProperty("Имя")]
        public string Name { get; set; }

        [ContextProperty("Шаблон")]
        public string Template { get; set; }

        [ContextProperty("ЗначенияПоУмолчанию")]
        public MapImpl Defaults { get; set; }

        // остальное пока подождет
    }
}
