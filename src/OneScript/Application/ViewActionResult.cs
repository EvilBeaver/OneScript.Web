using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
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
