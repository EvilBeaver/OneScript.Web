using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nustache.Core;
using ScriptEngine.Machine;

namespace OneScript.Mvc.Infrastructure
{
    public class OscriptContextPropertyGetterFactory : ValueGetterFactory
    {
        public override ValueGetter GetValueGetter(object target, Type targetType, string name)
        {
            var targetVal = target as IRuntimeContextInstance;
            if (targetVal == null)
                return null;

            return new OScriptContextPropertyGetter(targetVal, name);
        }
    }
}