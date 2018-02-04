using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("HTTPЗапросВходящий", "HTTPIncomingRequest")]
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
                    mapHdrs.SetIndexedValue(ValueFactory.Create(realObjectHeader.Key), ValueFactory.Create(realObjectHeader.Value));
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


        [ContextProperty("Заголовки")]
        public FixedMapImpl Headers { get; private set; }

        [ContextProperty("Cookies")]
        public FixedMapImpl Cookies { get; private set; }

        [ContextMethod("ПолучитьТелоКакПоток")]
        public GenericStream GetBodyAsStream()
        {
            return new GenericStream(_realObject.Body);
        }

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

        [ContextProperty("Метод")]
        public string Method => _realObject.Method;

        [ContextProperty("СтрокаЗапроса")]
        public string QueryString => _realObject.QueryString.Value;

        [ContextProperty("Путь")]
        public string Path => _realObject.Path;

        //[ContextProperty("Cookies")]
        //public object Cookies
    }
}
