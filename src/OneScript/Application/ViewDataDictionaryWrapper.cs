using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("СловарьДанныхПредставления")]
    public class ViewDataDictionaryWrapper : AutoContext<ViewDataDictionaryWrapper>, IEnumerable<KeyValuePair<string, IValue>>
    {
        private readonly Dictionary<string, IValue> _dictMap = new Dictionary<string, IValue>();

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
    }
}