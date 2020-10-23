/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    public class DynamicContextWrapper : DynamicObject, IEnumerable, IObjectWrapper
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

                result = CustomMarshaller.ConvertToDynamicCLRObject(_context.GetPropValue(propIdx));
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

                _context.SetPropValue(propIdx, CustomMarshaller.ConvertReturnValue(value, value.GetType()));

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

            var index = CustomMarshaller.ConvertReturnValue(indexes[0], indexes[0].GetType());
            result = CustomMarshaller.ConvertToDynamicCLRObject(_context.GetIndexedValue(index));
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (!_context.IsIndexed)
            {
                return false;
            }

            var index = CustomMarshaller.ConvertReturnValue(indexes[0], indexes[0].GetType());
            _context.SetIndexedValue(index, CustomMarshaller.ConvertReturnValue(value, value.GetType()));
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

            var methInfo = _context.GetMethodInfo(methIdx);
            var valueArgs = new IValue[methInfo.Params.Length];
            var passedArgs = args.Select(x => CustomMarshaller.ConvertReturnValue(x, x.GetType())).ToArray();
            for (int i = 0; i < valueArgs.Length; i++)
            {
                if (i < passedArgs.Length)
                    valueArgs[i] = passedArgs[i];
                else
                    valueArgs[i] = ValueFactory.CreateInvalidValueMarker();
            }

            IValue methResult;
            _context.CallAsFunction(methIdx, valueArgs, out methResult);
            result = methResult == null? null : CustomMarshaller.ConvertToDynamicCLRObject(methResult);

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
                yield return CustomMarshaller.ConvertToDynamicCLRObject(iValue);
            }
        }

        public override string ToString()
        {
            return ((IValue)_context).AsString();
        }

        public object UnderlyingObject => _context;
    }
}
