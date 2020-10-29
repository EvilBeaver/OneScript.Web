/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    public static class CustomMarshaller
    {
        public static IValue ConvertToIValueSafe(object objParam, Type type)
        {
            try
            {
                return ConvertReturnValue(objParam, type);
            }
            catch (NotSupportedException)
            {
                return ValueFactory.Create();
            }
        }

        // TODO - перенести в основной движок
        public static IValue ConvertReturnValue(object objParam, Type type)
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

        public static object ConvertToDynamicCLRObject(IValue value)
        {
            if (value.DataType == DataType.Object && !(value.AsObject() is IObjectWrapper))
            {
                return new DynamicContextWrapper(value.AsObject());
            }
            else
            {
                return ContextValuesMarshaller.ConvertToCLRObject(value);
            }
        }

        public static object ConvertToCLRObject(IValue value)
        {
            if (value.DataType == DataType.Object && !(value.AsObject() is IObjectWrapper))
            {
                return value.GetRawValue();
            }
            else
            {
                return ContextValuesMarshaller.ConvertToCLRObject(value);
            }
        }
    }
}
