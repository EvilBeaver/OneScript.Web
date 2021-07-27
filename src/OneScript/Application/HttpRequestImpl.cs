/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using Microsoft.AspNetCore.Http;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Описание входящего запроса HTTP
    /// </summary>
    [ContextClass("HttpЗапросВходящий", "HttpIncomingRequest")]
    public class HttpRequestImpl : AutoContext<HttpRequestImpl>
    {
        private readonly HttpRequest _realObject;
        private FormDataCollectionContext _formData;

        public HttpRequestImpl(HttpRequest request)
        {
            _realObject = request;
            UpdateHeaders();
            UpdateCookies();
        }

        private void UpdateHeaders()
        {
            var mapHdrs = new MapImpl();
            if (_realObject.Headers != null)
            {
                foreach (var realObjectHeader in _realObject.Headers)
                {
                    mapHdrs.SetIndexedValue(ValueFactory.Create(realObjectHeader.Key),
                        ValueFactory.Create(realObjectHeader.Value));
                }
            }

            Headers = new FixedMapImpl(mapHdrs);
        }

        private void UpdateCookies()
        {
            var cookieMap = new MapImpl();
            if (_realObject.Cookies != null)
            {
                foreach (var cookie in _realObject.Cookies)
                {
                    cookieMap.SetIndexedValue(ValueFactory.Create(cookie.Key),
                        ValueFactory.Create(cookie.Value));
                }
            }

            Cookies = new FixedMapImpl(cookieMap);
        }

        // для внутреннего пользования
        public HttpRequest RealObject => _realObject;

        /// <summary>
        /// ФиксированноеСоответствие. Заголовки входящего запроса
        /// </summary>
        [ContextProperty("Заголовки")]
        public FixedMapImpl Headers { get; private set; }

        /// <summary>
        /// ФиксированноеСоответствие. Cookies входящего запроса
        /// </summary>
        [ContextProperty("Cookies")]
        public FixedMapImpl Cookies { get; private set; }

        /// <summary>
        /// Получение тела запроса в виде потока для чтения
        /// </summary>
        /// <returns>Поток</returns>
        [ContextMethod("ПолучитьТелоКакПоток")]
        public GenericStream GetBodyAsStream()
        {
            return new GenericStream(_realObject.Body);
        }

        /// <summary>
        /// Коллекция переменных, переданных в качестве данных формы
        /// </summary>
        [ContextProperty("ДанныеФормы")]
        public FormDataCollectionContext FormData
        {
            get
            {
                if (_realObject.Form == null)
                    return null;

                if (_formData == null)
                {
                    _formData = new FormDataCollectionContext(_realObject.Form);
                }

                return _formData;
            }
        }

        /// <summary>
        /// Текущий метод HTTP
        /// </summary>
        [ContextProperty("Метод")]
        public string Method => _realObject.Method;

        /// <summary>
        /// Текущая строка запроса (QueryString)
        /// </summary>
        [ContextProperty("СтрокаЗапроса")]
        public string QueryString => _realObject.QueryString.Value;

        /// <summary>
        /// Коллекция параметров запроса (из СтрокиЗапроса)
        /// </summary>
        [ContextMethod("ПараметрыЗапроса")]
        public MapImpl QueryParameters()
        {
            var result = new MapImpl();
            foreach (var (key, value) in _realObject.Query)
            {
                var valueToInsert = value.Count > 1 ? 
                    new ArrayImpl(value.Select(ValueFactory.Create)) :
                    ValueFactory.Create(value);

                result.Insert(
                    ValueFactory.Create(key),
                    valueToInsert);
            }

            return result;
        }

        /// <summary>
        /// Путь текущего ресурса
        /// </summary>
        [ContextProperty("Путь")]
        public string Path => _realObject.Path;
    }
}
