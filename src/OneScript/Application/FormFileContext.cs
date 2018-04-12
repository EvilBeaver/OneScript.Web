﻿using System;
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

        [ContextProperty("Имя")]
        public string Name => _realObject.Name;

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
            return new GenericStream(stream, true);
        }
    }
}
