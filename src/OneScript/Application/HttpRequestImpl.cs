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

        [ContextProperty("Заголовки")]
        public FixedMapImpl Headers { get; private set; }

        [ContextMethod("УстановитьЗаголовки")]
        public void SetHeaders(MapImpl headers)
        {
            _realObject.Headers.Clear();
            foreach (var header in headers)
            {
                _realObject.Headers.Add(header.Key.AsString(), header.Value.AsString());
            }
            Headers = new FixedMapImpl(headers);
        }

        [ContextMethod("ПолучитьТелоКакПоток")]
        public GenericStream GetBodyAsStream()
        {
            return new GenericStream(_realObject.Body);
        }

        [ContextProperty("Метод")]
        public string Method => _realObject.Method;

        [ContextProperty("СтрокаЗапроса")]
        public string QueryString => _realObject.QueryString.Value;

        [ContextProperty("Путь")]
        public string Path => _realObject.Path;
    }
}
