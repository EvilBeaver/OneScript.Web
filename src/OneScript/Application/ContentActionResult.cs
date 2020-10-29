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
    [ContextClass("РезультатДействияСодержимое")]
    public class ContentActionResult : AutoContext<ContentActionResult>, IObjectWrapper
    {
        private readonly ContentResult _contentResult = new ContentResult();

        [ContextProperty("Содержимое")]
        public string Content
        {
            get => _contentResult.Content;
            set => _contentResult.Content = value;
        }

        [ContextProperty("ТипСодержимого")]
        public string ContentType
        {
            get => _contentResult.ContentType;
            set => _contentResult.ContentType = value;
        }

        [ContextProperty("КодСостояния")]
        public int StatusCode
        {
            get => _contentResult.StatusCode ?? 200;
            set => _contentResult.StatusCode = value;
        }
        
        [ScriptConstructor]
        public static ContentActionResult Constructor()
        {
            return new ContentActionResult();
        }

        public object UnderlyingObject => _contentResult;
    }
}
