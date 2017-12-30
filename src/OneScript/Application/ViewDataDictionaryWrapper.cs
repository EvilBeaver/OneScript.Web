using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("КоллекцияДанныхПредставления")]
    public class ViewDataDictionaryWrapper : ContextIValueImpl, IObjectWrapper
    {
        private readonly ViewDataDictionary _source;

        public ViewDataDictionaryWrapper(ViewDataDictionary source)
        {
            _source = source ?? throw new ArgumentNullException();
        }

        public IValue this[string index]
        {
            get { return (IValue)_source[index]; }
            set { _source[index] = value; }
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index)
        {
            return (IValue)_source[index.AsString()];
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            _source[index.AsString()] = val.GetRawValue();
        }

        public object Model
        {
            get => _source.Model;
            set => _source.Model = MapValueToModelType(value);
        }

        private object MapValueToModelType(object value)
        {
            if (value is IValue)
            {
                var iValue = value as IValue;
                if (iValue.DataType == DataType.Object)
                {
                    return new DynamicContextWrapper(iValue.AsObject());
                }
                else
                {
                    return ContextValuesMarshaller.ConvertToCLRObject(iValue.GetRawValue());
                }
            }

            return value;
        }

        public object UnderlyingObject => _source;
    }
}