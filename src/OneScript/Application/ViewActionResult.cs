/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("РезультатДействияПредставление")]
    public class ViewActionResult : AutoContext<ViewActionResult>, IActionResult, IObjectWrapper
    {
        [ContextProperty("ИмяШаблона")]
        public string ViewName { get; set; }

        [ContextProperty("ТипСодержимого")]
        public string ContentType { get; set; }

        [ContextProperty("КодСостояния")]
        public int StatusCode { get; set; }

        [ContextProperty("ДанныеПредставления")]
        public ViewDataDictionaryWrapper ViewData { get; set; }
        
        public ViewResult CreateExecutableResult()
        {
            var result = new ViewResult();
            result.ViewName = ViewName;
            result.ContentType = ContentType;
            result.StatusCode = StatusCode == 0 ? default(int?) : StatusCode;
            result.ViewData = ViewData?.GetDictionary();

            return result;
        }

        [ScriptConstructor]
        public static ViewActionResult Constructor()
        {
            return new ViewActionResult();
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var viewRes = CreateExecutableResult();
            return viewRes.ExecuteResultAsync(context);
        }

        // TODO: Костыль. Маршаллер выдает исключение при возврате через Invoke, если тип не Wrapper.
        public object UnderlyingObject => this;
        
    }
}
