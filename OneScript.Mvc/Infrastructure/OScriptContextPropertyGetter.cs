using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nustache.Core;
using ScriptEngine.Machine;

namespace OneScript.Mvc.Infrastructure
{
    public class OScriptContextPropertyGetter : ValueGetter
    {
        private readonly IRuntimeContextInstance _target;
        private readonly string _propertyName;

        public OScriptContextPropertyGetter(IRuntimeContextInstance target, string propertyName)
        {
            _target = target;
            _propertyName = propertyName;
        }

        public override object GetValue()
        {
            object value = null;
            try
            {
                int idx = _target.FindProperty(_propertyName);
                value = _target.GetPropValue(idx);
            }
            catch (PropertyAccessException e)
            {
                value = null;
            }

            return value;
        }
    }
}