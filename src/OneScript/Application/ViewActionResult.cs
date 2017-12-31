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
    [ContextClass("РезультатДействияСтраница")]
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
            result.ViewData = GetDictionary();

            return result;
        }

        private ViewDataDictionary GetDictionary()
        {
            if (ViewData == null)
                return null;

            var model = ViewData.Model;
            var realDict = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            if (model != null)
            {
                if (model.DataType == DataType.Object)
                {
                    realDict.Model = new DynamicContextWrapper(model.AsObject());
                }
                else
                {
                    realDict.Model = ContextValuesMarshaller.ConvertToCLRObject(model.GetRawValue());
                }
            }

            foreach (var iValItem in ViewData)
            {
                var iVal = iValItem.Value as IValue;
                if (iVal.DataType == DataType.Object)
                {
                    realDict[iValItem.Key] = new DynamicContextWrapper(iVal.AsObject());
                }
                else
                {
                    realDict[iValItem.Key] = ContextValuesMarshaller.ConvertToCLRObject(iVal.GetRawValue());
                }
            }

            return realDict;
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
