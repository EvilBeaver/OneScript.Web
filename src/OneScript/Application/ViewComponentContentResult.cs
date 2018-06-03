using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("РезультатКомпонентаСодержимое","ViewComponentContentResult")]
    public class ViewComponentContentResult : AutoContext<ContentActionResult>, IObjectWrapper
    {
        private ContentViewComponentResult _realObject;

        [ContextProperty("Содержимое")]
        public string Content { get; set; }

        public object UnderlyingObject
        {
            get
            {
                if (_realObject == null)
                {
                    _realObject = new ContentViewComponentResult(Content);
                }

                return _realObject;
            }
        }

        [ScriptConstructor]
        public static ViewComponentContentResult Constructor(string content)
        {
            return new ViewComponentContentResult()
            {
                Content = content
            };
        }
    }
}
