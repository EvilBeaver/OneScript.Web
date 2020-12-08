/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Описание исходящего HTTP-ответа
    /// </summary>
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

        /// <summary>
        /// Фиксированное соответствие. Заголовки исходящего запроса.
        /// Для установки заголовков см. метод УстановитьЗаголовки.
        /// </summary>
        [ContextProperty("Заголовки")]
        public FixedMapImpl Headers { get; private set; }

        /// <summary>
        /// Возвращаемый код состояния.
        /// </summary>
        [ContextProperty("КодСостояния")]
        public int StatusCode { get=>_realObject.StatusCode; set => _realObject.StatusCode = value; }

        /// <summary>
        /// Возвращаемый тип содержимого (Content-type)
        /// </summary>
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

        /// <summary>
        /// Показывает, была ли начата отправка ответа клиенту.
        /// Если да, то модификация ответа (установка тела или заголовков) приведёт к исключению.
        /// </summary>
        [ContextProperty("ОтправкаНачата", "HasStarted")]
        public bool HasStarted => _realObject.HasStarted;

        // для внутреннего пользования
        public HttpResponse RealObject => _realObject;

        /// <summary>
        /// Устанавливает заголовки текущего ответа
        /// </summary>
        /// <param name="headers">Соответствие. Устанавливаемые заголовки</param>
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

        /// <summary>
        /// Добавляет значение Cookie в ответ
        /// </summary>
        /// <param name="key">Имя параметра</param>
        /// <param name="value">Значение параметра</param>
        /// <param name="options">Опции Cookie</param>
        [ContextMethod("УстановитьCookie")]
        public void SetCookie(string key, string value, CookieOptionsWraper options = null)
        {
            if (options == null)
                _realObject.Cookies.Append(key, value);
            else
                _realObject.Cookies.Append(key, value, (CookieOptions)options.UnderlyingObject);
        }

        /// <summary>
        /// Удаление значения Cookie
        /// </summary>
        /// <param name="key">Имя параметра</param>
        /// <param name="options">Опции Cookie</param>
        [ContextMethod("УдалитьCookie")]
        public void RemoveCookie(string key, CookieOptionsWraper options = null)
        {
            if (options == null)
                _realObject.Cookies.Delete(key);
            else
                _realObject.Cookies.Delete(key, (CookieOptions)options.UnderlyingObject);
        }

        /// <summary>
        /// Открывает Поток, применяемый для наполнения тела ответа.
        /// </summary>
        /// <returns>Поток</returns>
        [ContextMethod("ПолучитьТелоКакПоток")]
        public GenericStream GetBodyAsStream()
        {
            return new GenericStream(_realObject.Body);
        }

        /// <summary>
        /// Устанавливает тело ответа из строки с заданной кодировкой.
        /// </summary>
        /// <param name="body">Тело ответа</param>
        /// <param name="encoding">Кодировка текста ответа</param>
        [ContextMethod("УстановитьТелоИзСтроки")]
        public void SetBodyFromString(string body, IValue encoding = null)
        {
            var enc = encoding == null? new UTF8Encoding(false) : TextEncodingEnum.GetEncoding(encoding, false);

            using (var writer = new StreamWriter(_realObject.Body, enc))
            {
                writer.Write(body);
            }
        }

        /// <summary>
        /// Устанавливает ДвоичныеДанные в качестве тела ответа
        /// </summary>
        /// <param name="data">Данные</param>
        [ContextMethod("УстановитьТелоИзДвоичныхДанных")]
        public void SetBodyFromBinaryData(BinaryDataContext data)
        {
            _realObject.Body.Write(data.Buffer, 0, data.Buffer.Length);
        }
    }
}
