/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using Microsoft.AspNetCore.Http;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("ПараметрыCookie")]
    public class CookieOptionsWraper : AutoContext<CookieOptionsWraper>, IObjectWrapper
    {
        private readonly CookieOptions _options;

        public CookieOptionsWraper()
        {
            _options = new CookieOptions();
        }

        [ContextProperty("Домен")]
        public string Domain
        {
            get { return _options.Domain; }
            set { _options.Domain = value; }
        }

        [ContextProperty("Путь")]
        public string Path
        {
            get { return _options.Path; }
            set { _options.Path = value; }
        }

        [ContextProperty("ТолькоДляHttp")]
        public bool HttpOnly
        {
            get { return _options.HttpOnly; }
            set { _options.HttpOnly = value; }
        }

        [ContextProperty("БезопасныйРежим")]
        public bool Secure
        {
            get { return _options.Secure; }
            set { _options.Secure = value; }
        }

        /// <summary>
        /// Устанавливает срок действия Cookie
        /// <param name="offset">Строка. Срок действия в формате .NET см. https://msdn.microsoft.com/ru-ru/library/bb351654(v=vs.110).aspx </param>
        /// </summary>
        /// <param name="offset"></param>
        [ContextMethod("УстановитьСрокДействия")]
        public void SetExpiration(IValue offset)
        {
            if (offset.DataType == DataType.Undefined)
            {
                _options.Expires = null;
            }
            else
            {
                _options.Expires = DateTimeOffset.Parse(offset.AsString());
            }
        }

        public object UnderlyingObject => _options;

        [ScriptConstructor]
        public CookieOptionsWraper Create()
        {
            return new CookieOptionsWraper();
        }
    }
}
