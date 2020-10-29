/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Класс предназначен для описания URL-шаблонов, по которым будет откликаться веб-приложение.
    ///  Пример шаблона с 3-мя переменными. Переменные controller и action являются предопределенными.
    /// /{controller}/{action}/{id?}
    /// </summary>
    [ContextClass("КоллекцияМаршрутов","RoutesCollection")]
    public class RoutesCollectionContext : AutoContext<RoutesCollectionContext>, IEnumerable<RouteDescriptionContext>, ICollectionContext
    {
        List<RouteDescriptionContext> _routes = new List<RouteDescriptionContext>();

        /// <summary>
        /// Добавление шаблона URL в коллекцию.
        /// </summary>
        /// <param name="name">Имя маршрута. Позволяет строить исходящие URL по заданному шаблону</param>
        /// <param name="template">Шаблон адреса. Формируется по правилам шаблонов ASP.NET MVC Core</param>
        /// <param name="defaults">Соответствие. Определяет значения по-умолчанию для переменных маршрута.</param>
        /// <example>
        /// Умолчания = Новый Соответствие;
        /// Умолчания.Вставить("shopId", 12344); // если магазин не указан - взять магазин 12344
        /// КоллекцияМаршрутов.Добавить("ПоМагазину","{controller}/{action}/{shopId}", Умолчания);
        /// </example>
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
