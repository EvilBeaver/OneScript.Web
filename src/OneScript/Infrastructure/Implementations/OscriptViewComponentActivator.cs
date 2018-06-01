using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Reflection;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OscriptViewComponentActivator : IViewComponentActivator
    {
        private readonly Dictionary<Type, Func<object>> _constructorsCache;

        public OscriptViewComponentActivator()
        {
            _constructorsCache = new Dictionary<Type, Func<object>>();
        }

        public object Create(ViewComponentContext context)
        {
            var type = context.ViewComponentDescriptor.TypeInfo;
            if (_constructorsCache.TryGetValue(type.AsType()
                , out var constructor))
            {
                return constructor();
            }

            var cInfo = type.GetConstructors().OfType<ReflectedConstructorInfo>().FirstOrDefault();
            if(cInfo == null)
                throw new RuntimeException($"No constructor found in type {type}");

            object Launch() => cInfo.Invoke(new object[0]);
            _constructorsCache[type] = Launch;

            return Launch();
        }

        public void Release(ViewComponentContext context, object viewComponent)
        {
            
        }
    }
}
