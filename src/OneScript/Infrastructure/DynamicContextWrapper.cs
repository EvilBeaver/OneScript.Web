using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    public class DynamicContextWrapper : DynamicObject, IEnumerable
    {
        private readonly IRuntimeContextInstance _context;

        public DynamicContextWrapper(IRuntimeContextInstance context)
        {
            _context = context;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                var propIdx = _context.FindProperty(binder.Name);
                if (!_context.IsPropReadable(propIdx))
                {
                    result = null;
                    return false;
                }

                result = ContextValuesMarshaller.ConvertToCLRObject(_context.GetPropValue(propIdx));
                return true;
            }
            catch (PropertyAccessException)
            {
                result = null;
                return false;
            }
            catch (ValueMarshallingException)
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            try
            {
                var propIdx = _context.FindProperty(binder.Name);
                if (!_context.IsPropWritable(propIdx))
                {
                    return false;
                }

                _context.SetPropValue(propIdx, ConvertReturnValue(value, value.GetType()));

                return true;
            }
            catch (PropertyAccessException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (!_context.IsIndexed)
            {
                result = null;
                return false;
            }

            var index = ConvertReturnValue(indexes[0], indexes[0].GetType());
            result = ContextValuesMarshaller.ConvertToCLRObject(_context.GetIndexedValue(index));
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (!_context.IsIndexed)
            {
                return false;
            }

            var index = ConvertReturnValue(indexes[0], indexes[0].GetType());
            _context.SetIndexedValue(index, ConvertReturnValue(value, value.GetType()));
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            int methIdx;
            try
            {
                methIdx = _context.FindMethod(binder.Name);
            }
            catch (MethodAccessException)
            {
                result = null;
                return false;
            }

            var valueArgs = args.Select(x => ConvertReturnValue(x, x.GetType())).ToArray();
            IValue methResult;
            _context.CallAsFunction(methIdx, valueArgs, out methResult);
            result = ContextValuesMarshaller.ConvertToCLRObject(methResult);

            return true;

        }

        public IEnumerator GetEnumerator()
        {
            if (!(_context is IEnumerable<IValue>))
            {
                throw RuntimeException.IteratorIsNotDefined();
            }

            var enumer = (IEnumerable<IValue>) _context;
            foreach (var iValue in enumer)
            {
                if (iValue.DataType == DataType.Object)
                {
                    yield return new DynamicContextWrapper((IRuntimeContextInstance)iValue);
                }
                else
                {
                    yield return ContextValuesMarshaller.ConvertToCLRObject(iValue);
                }
            }
        }

        // TODO - перенести в основной движок
        private static IValue ConvertReturnValue(object objParam, Type type)
        {
            if (objParam == null)
                return ValueFactory.Create();

            if (type == typeof(IValue))
            {
                return (IValue)objParam;
            }
            else if (type == typeof(string))
            {
                return ValueFactory.Create((string)objParam);
            }
            else if (type == typeof(int))
            {
                return ValueFactory.Create((int)objParam);
            }
            else if (type == typeof(uint))
            {
                return ValueFactory.Create((uint)objParam);
            }
            else if (type == typeof(long))
            {
                return ValueFactory.Create((long)objParam);
            }
            else if (type == typeof(ulong))
            {
                return ValueFactory.Create((ulong)objParam);
            }
            else if (type == typeof(decimal))
            {
                return ValueFactory.Create((decimal)objParam);
            }
            else if (type == typeof(double))
            {
                return ValueFactory.Create((decimal)(double)objParam);
            }
            else if (type == typeof(DateTime))
            {
                return ValueFactory.Create((DateTime)objParam);
            }
            else if (type == typeof(bool))
            {
                return ValueFactory.Create((bool)objParam);
            }
            else if (type.IsEnum)
            {
                var wrapperType = typeof(CLREnumValueWrapper<>).MakeGenericType(new Type[] { type });
                var constructor = wrapperType.GetConstructor(new Type[] { typeof(EnumerationContext), type, typeof(DataType) });
                var osValue = (EnumerationValue)constructor.Invoke(new object[] { null, objParam, DataType.Enumeration });
                return osValue;
            }
            else if (typeof(IRuntimeContextInstance).IsAssignableFrom(type))
            {
                return ValueFactory.Create((IRuntimeContextInstance)objParam);
            }
            else
            {
                throw new NotSupportedException("Type is not supported");
            }

        }
    }
}
