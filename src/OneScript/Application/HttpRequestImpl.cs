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
            var mapHdrs = new MapImpl();
            foreach (var realObjectHeader in _realObject.Headers)
            {
                mapHdrs.SetIndexedValue(ValueFactory.Create(realObjectHeader.Key),ValueFactory.Create(realObjectHeader.Value));
            }
            Headers = new FixedMapImpl(mapHdrs);
        }

        // для внутреннего пользования
        public HttpRequest RealObject => _realObject;


        [ContextProperty("Заголовки")]
        public FixedMapImpl Headers { get; }
        
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
    }
}
