/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("СловарьДанныхПредставления")]
    public class ViewDataDictionaryWrapper : AutoContext<ViewDataDictionaryWrapper>, IEnumerable<KeyValuePair<string, IValue>>
    {
        private readonly Dictionary<string, IValue> _dictMap = new Dictionary<string, IValue>();
        private ViewDataDictionary _realDictionary;

        public ViewDataDictionaryWrapper()
        {
        }

        public ViewDataDictionaryWrapper(ViewDataDictionary value)
        {
            _realDictionary = value;
        }

        public IValue this[string index]
        {
            get { return _dictMap[index]; }
            set { _dictMap[index] = value; }
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index)
        {
            return _dictMap[index.AsString()];
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            _dictMap[index.AsString()] = val;
        }

        [ContextProperty("Модель")]
        public IValue Model { get; set; }

        public IEnumerator<KeyValuePair<string, IValue>> GetEnumerator()
        {
            return _dictMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dictMap).GetEnumerator();
        }

        [ScriptConstructor]
        public static ViewDataDictionaryWrapper Create()
        {
            return new ViewDataDictionaryWrapper();
        }

        public ViewDataDictionary GetDictionary()
        {
            var model = Model;
            var realDict = _realDictionary ?? new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            if (model != null)
            {
                if (model.DataType == DataType.Object)
                {
                    realDict.Model = new DynamicContextWrapper(model.AsObject());
                }
                else
                {
                    realDict.Model = ContextValuesMarshaller.ConvertToCLRObject(model.GetRawValue());
                }
            }

            foreach (var iValItem in _dictMap)
            {
                var iVal = iValItem.Value;
                if (iVal.DataType == DataType.Object)
                {
                    realDict[iValItem.Key] = new DynamicContextWrapper(iVal.AsObject());
                }
                else
                {
                    realDict[iValItem.Key] = ContextValuesMarshaller.ConvertToCLRObject(iVal.GetRawValue());
                }
            }

            return realDict;
        }
    }
}
