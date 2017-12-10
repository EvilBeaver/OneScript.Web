using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    public class RoutesCollectionContext : AutoContext<RoutesCollectionContext>, IEnumerable<RouteDescriptionContext>, ICollectionContext
    {
        List<RouteDescriptionContext> _routes = new List<RouteDescriptionContext>();

        [ContextMethod("Добавить")]
        public void Add(string name, string template, MapImpl defaults = null)
        {
            var route = new RouteDescriptionContext()
            {
                Name = name,
                Template = template,
                Defaults = defaults
            };

            _routes.Add(route);
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index)
        {
            if (index.DataType == DataType.Number)
            {
                return _routes[(int) index.AsNumber()];
            }
            if (index.DataType == DataType.String)
            {
                var value = _routes.Find(x => x.Name == index.AsString());
                if (value == null)
                    return ValueFactory.Create();

                return value;
            }
            throw RuntimeException.InvalidArgumentType(nameof(index));
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<RouteDescriptionContext> GetEnumerator()
        {
            return _routes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count()
        {
            return _routes.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(_routes);
        }
    }
}
