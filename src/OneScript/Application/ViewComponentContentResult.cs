/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("РезультатКомпонентаСодержимое","ViewComponentContentResult")]
    public class ViewComponentContentResult : AutoContext<ContentActionResult>, IObjectWrapper, IViewComponentResult
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
        public static ViewComponentContentResult Constructor(IValue content)
        {
            return new ViewComponentContentResult()
            {
                Content = content.AsString()
            };
        }

        public void Execute(ViewComponentContext context)
        {
            ExecuteAsync(context).GetAwaiter().GetResult();
        }

        public Task ExecuteAsync(ViewComponentContext context)
        {
            return _realObject.ExecuteAsync(context);
        }
    }
}
