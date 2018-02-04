using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("HttpОтветИсходящий","HttpOutgoingResponse")]
    public class HttpResponseImpl : AutoContext<HttpResponseImpl>
    {
        private readonly HttpResponse _realObject;

        public HttpResponseImpl(HttpResponse realObject)
        {
            _realObject = realObject;
            UpdateHeaders();
        }

        private void UpdateHeaders()
        {
            var mapHdrs = new MapImpl();
            foreach (var realObjectHeader in _realObject.Headers)
            {
                mapHdrs.SetIndexedValue(ValueFactory.Create(realObjectHeader.Key), ValueFactory.Create(realObjectHeader.Value));
            }
            Headers = new FixedMapImpl(mapHdrs);
        }
        
        [ContextProperty("Заголовки")]
        public FixedMapImpl Headers { get; private set; }

        [ContextProperty("КодСостояния")]
        public int StatusCode { get=>_realObject.StatusCode; set => _realObject.StatusCode = value; }

        [ContextProperty("ТипСодержимого")]
        public string ContentType
        {
            get { return _realObject.ContentType; }
            set
            {
                _realObject.ContentType = value;
                UpdateHeaders();
            }

        }

        // для внутреннего пользования
        public HttpResponse RealObject => _realObject;

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

        [ContextMethod("УстановитьCookie")]
        public void SetCookie(string key, string value, CookieOptionsWraper options = null)
        {
            if (options == null)
                _realObject.Cookies.Append(key, value);
            else
                _realObject.Cookies.Append(key, value, (CookieOptions)options.UnderlyingObject);
        }

        [ContextMethod("УдалитьCookie")]
        public void RemoveCookie(string key, CookieOptionsWraper options = null)
        {
            if (options == null)
                _realObject.Cookies.Delete(key);
            else
                _realObject.Cookies.Delete(key, (CookieOptions)options.UnderlyingObject);
        }

        [ContextMethod("ПолучитьТелоКакПоток")]
        public GenericStream GetBodyAsStream()
        {
            return new GenericStream(_realObject.Body);
        }

        [ContextMethod("УстановитьТелоИзСтроки")]
        public void SetBodyFromString(string body, IValue encoding = null)
        {
            var enc = encoding == null? new UTF8Encoding(false) : TextEncodingEnum.GetEncoding(encoding, false);

            using (var writer = new StreamWriter(_realObject.Body, enc))
            {
                writer.Write(body);
            }
        }

        [ContextMethod("УстановитьТелоИзДвоичныхДанных")]
        public void SetBodyFromBinaryData(BinaryDataContext data)
        {
            _realObject.Body.Write(data.Buffer, 0, data.Buffer.Length);
        }
    }
}
