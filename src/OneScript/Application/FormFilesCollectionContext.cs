/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Коллекция переданных с клиента файлов.
    /// Обращение к коллекции возможно по числовому индексу или имени поля-файла.
    /// </summary>
    [ContextClass("КоллекцияФайловФормы")]
    public class FormFilesCollectionContext : AutoContext<FormFilesCollectionContext>, ICollectionContext, IEnumerable<FormFileContext>
    {
        private readonly List<FormFileContext> _data = new List<FormFileContext>();
        private readonly Dictionary<string, int> _indexer = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public FormFilesCollectionContext(IFormFileCollection data)
        {
            int i = 0;
            foreach (var fFile in data)
            {
                _data.Add(new FormFileContext(fFile));
                _indexer[fFile.Name] = i++;
            }
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index)
        {
            if (index.DataType == DataType.Number)
                return this[(int) index.AsNumber()];

            if (index.DataType == DataType.String)
                return this[index.AsString()];

            throw RuntimeException.InvalidArgumentType(nameof(index));
        }

        public FormFileContext this[int index] => _data[index];

        public FormFileContext this[string index]
        {
            get
            {
                var intIdx = _indexer[index];
                return _data[intIdx];
            }
        }

        [ContextMethod("Количество")]
        public int Count()
        {
            return _data.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public IEnumerator<FormFileContext> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
