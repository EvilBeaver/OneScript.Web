/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("ОписаниеМаршрута", "RouteDescription")]
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
