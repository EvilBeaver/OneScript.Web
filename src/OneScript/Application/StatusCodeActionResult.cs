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
    [ContextClass("РезультатДействияКодСостояния")]
    public class StatusCodeActionResult : AutoContext<StatusCodeActionResult>, IObjectWrapper
    {

        public StatusCodeActionResult(StatusCodeResult result)
        {
            UnderlyingObject = result;
        }

        [ScriptConstructor]
        public static StatusCodeActionResult Constructor(int statusCode)
        {
            var scResult = new StatusCodeResult(statusCode);
            return new StatusCodeActionResult(scResult);
        }

        public object UnderlyingObject { get; }
    }
}
