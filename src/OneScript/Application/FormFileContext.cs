/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using Microsoft.AspNetCore.Http;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Описание файла формы
    /// </summary>
    [ContextClass("ФайлФормы")]
    public class FormFileContext : AutoContext<FormFileContext>
    {
        private readonly IFormFile _realObject;
        private readonly Lazy<FixedMapImpl> _headers;

        public FormFileContext(IFormFile realObject)
        {
            _realObject = realObject;

            _headers = new Lazy<FixedMapImpl>(() =>
                {
                    var mapHdrs = new MapImpl();
                    foreach (var realObjectHeader in _realObject.Headers)
                    {
                        mapHdrs.SetIndexedValue(ValueFactory.Create(realObjectHeader.Key), ValueFactory.Create(realObjectHeader.Value));
                    }
                    return new FixedMapImpl(mapHdrs);
                }
            );
        }

        /// <summary>
        /// Имя поля формы, в котором был получен данный файл
        /// </summary>
        [ContextProperty("Имя")]
        public string Name => _realObject.Name;

        /// <summary>
        /// Имя переданного файла, как указано в заголовке Content-Disposition
        /// </summary>
        [ContextProperty("ИмяФайла")]
        public string FileName => _realObject.FileName;

        [ContextProperty("Размер")]
        public long Length => _realObject.Length;

        /// <summary>
        /// Заголовки данного файла.
        /// </summary>
        [ContextProperty("Заголовки")]
        public FixedMapImpl Headers => _headers.Value;

        /// <summary>
        /// Значение заголовка Content-type для данного файла.
        /// </summary>
        [ContextProperty("ТипСодержимого")]
        public string ContentType => _realObject.ContentType;

        /// <summary>
        /// Значение заголовка Content-disposition для данного файла.
        /// </summary>
        [ContextProperty("РасположениеСодержимого")]
        public string ContentDisposition => _realObject.ContentDisposition;

        /// <summary>
        /// Открывает поток для чтения содержимого файла
        /// </summary>
        /// <returns></returns>
        [ContextMethod("ОткрытьПотокДляЧтения")]
        public GenericStream OpenReadStream()
        {
            var stream = _realObject.OpenReadStream();
            return new GenericStream(stream);
        }
    }
}
