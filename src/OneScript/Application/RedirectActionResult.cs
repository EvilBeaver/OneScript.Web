/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using Microsoft.AspNetCore.Mvc;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("РезультатДействияПеренаправление")]
    public class RedirectActionResult : AutoContext<RedirectActionResult>, IObjectWrapper
    {
        private readonly RedirectResult _result;

        public RedirectActionResult(string url, bool permanent)
        {
            _result = new RedirectResult(url, permanent);
        }

        public object UnderlyingObject => _result;


        [ScriptConstructor]
        public static RedirectActionResult Create(string url, bool permanent = false)
        {
            return new RedirectActionResult(url, permanent);
        }
    }
}
