/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Значения полей формы во входящем запросе.
    /// Обращения к полям формы выполняется с помощью оператора [].
    /// В качестве индекса используется имя поля.
    /// </summary>
    [ContextClass("КоллекцияДанныхФормы")]
    public class FormDataCollectionContext : AutoContext<FormDataCollectionContext>, ICollectionContext, IEnumerable<KeyAndValueImpl>
    {
        private readonly IFormCollection _data;
        private readonly FormFilesCollectionContext _files;

        public FormDataCollectionContext(IFormCollection data)
        {
            _data = data;
            if(_data.Files != null)
                _files = new FormFilesCollectionContext(data.Files);
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index)
        {
            return ValueFactory.Create(_data[index.AsString()]);
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            throw RuntimeException.PropIsNotWritableException("index");
        }

        /// <summary>
        /// Коллекция загружаемых файлов (upload)
        /// </summary>
        [ContextProperty("Файлы")]
        public FormFilesCollectionContext Files => _files;

        [ContextMethod("Количество")]
        public int Count()
        {
            return _data.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public IEnumerator<KeyAndValueImpl> GetEnumerator()
        {
            foreach (var kv in _data)
            {
                yield return new KeyAndValueImpl(ValueFactory.Create(kv.Key), ValueFactory.Create(kv.Value.ToString()));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
