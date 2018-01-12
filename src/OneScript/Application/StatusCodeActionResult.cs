using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
