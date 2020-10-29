/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("КомпонентПредставления")]
    [NonViewComponent]
    public class ScriptedViewComponent : AutoScriptDrivenObject<ScriptedViewComponent>
    {
        public const string InvokeMethodNameRu = "ОбработкаВызова";
        public const string InvokeMethodNameEn = "CallProcessing";

        private Func<IDictionary<string, object>, object> _invoker;

        private IUrlHelper _url;
        private SessionImpl _session;
        private ViewComponentContext _ctx;
        private ViewDataDictionaryWrapper _osViewData;

        public ScriptedViewComponent(LoadedModule module, string dynamicTypeName) : base(module, true)
        {
            var td = TypeManager.RegisterType(dynamicTypeName, typeof(ScriptedViewComponent));           
            DefineType(td);
            InitOwnData();


            var methId = GetScriptMethod(InvokeMethodNameRu, InvokeMethodNameEn);
            if (methId == -1)
            {
                throw new RuntimeException("Invoke method not found");
            }

            object Invoker(IDictionary<string, object> arguments)
            {
                var args = MapArguments(methId, arguments);
                var result = CallScriptMethod(methId, args);
                return CustomMarshaller.ConvertToCLRObject(result);
            }
            
            _invoker = Invoker;
        }

        private IValue[] MapArguments(int methId, IDictionary<string, object> arguments)
        {
            var parameters = GetMethodInfo(methId + GetOwnMethodCount()).Params;
            IValue[] args = new IValue[parameters.Length];

            if (parameters.Length == 0 && arguments.Count != 0)
            {
                throw RuntimeException.TooManyArgumentsPassed();
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                var obj = arguments[parameters[i].Name];
                if (obj == null)
                {
                    args[i] = ValueFactory.Create();
                    continue;
                }

                var type = obj is IValue ? typeof(IValue) : obj.GetType();

                if (obj is DynamicContextWrapper dyn)
                {
                    obj = dyn.UnderlyingObject;

                    if (type == typeof(DynamicContextWrapper))
                        type = obj.GetType();
                }

                args[i] = CustomMarshaller.ConvertToIValueSafe(obj, type);
            }

            return args;
        }

        private void OnContextChange()
        {
            HttpRequest = new HttpRequestImpl(ComponentContext.ViewContext.HttpContext.Request);
            HttpResponse = new HttpResponseImpl(ComponentContext.ViewContext.HttpContext.Response);
            ViewData = new ViewDataDictionaryWrapper(ComponentContext.ViewData);

            var routeData = _ctx.ViewContext.RouteData;
            if (routeData != null)
            {
                RouteValues = new MapImpl();
                var rv = RouteValues.AsObject();
                foreach (var routeValue in routeData.Values)
                {
                    rv.SetIndexedValue(
                        ValueFactory.Create(routeValue.Key),
                        CustomMarshaller.ConvertToIValueSafe(routeValue.Value, routeValue.Value.GetType())
                    );
                }
            }
            else
                RouteValues = ValueFactory.Create();
        }

        /// <summary>
        /// Входящий запрос HTTP
        /// </summary>
        [ContextProperty("ЗапросHttp")]
        public HttpRequestImpl HttpRequest { get; private set; }

        /// <summary>
        /// Исходящий ответ HTTP
        /// </summary>
        [ContextProperty("ОтветHttp")]
        public HttpResponseImpl HttpResponse { get; private set; }

        /// <summary>
        /// Действующие значения маршрута для текущего вызова.
        /// Тип: Соответствие или Неопределено.
        /// Ключами соответствия являются переменные маршрута.
        /// </summary>
        [ContextProperty("ЗначенияМаршрута")]
        public IValue RouteValues { get; private set; }

        /// <summary>
        /// Данные http-сессии. Механизм сессий использует Cookies для привязки сессии и InMemory хранилище для данных сессии.
        /// </summary>
        [ContextProperty("Сессия")]
        public SessionImpl Session
        {
            get
            {
                if (_session == null)
                    _session = new SessionImpl(ComponentContext.ViewContext.HttpContext.Session);
                return _session;
            }
        }

        public IUrlHelper Url
        {
            get
            {
                if (_url == null)
                {
                    var services = ComponentContext?.ViewContext?.HttpContext?.RequestServices;
                    if (services == null)
                    {
                        return null;
                    }

                    var factory = services.GetRequiredService<IUrlHelperFactory>();
                    _url = factory.GetUrlHelper(ComponentContext.ViewContext);
                }

                return _url;
            }
        }

        /// <summary>
        /// Специализированный объект, предназначенный для передачи данных в генерируемое Представление.
        /// Элементы коллекции доступны в Представлении через свойства ViewBag и ViewData.
        /// </summary>
        [ContextProperty("ДанныеПредставления")]
        public ViewDataDictionaryWrapper ViewData
        {
            get => _osViewData ?? (_osViewData = new ViewDataDictionaryWrapper());
            set => _osViewData = value ?? throw new ArgumentException();
        }

        public object Invoke(IDictionary<string, object> arguments)
        {
            return _invoker(arguments);
        }

        [ViewComponentContext]
        public ViewComponentContext ComponentContext
        {
            get { return _ctx; }
            set
            {
                _ctx = value;
                OnContextChange();
            }
        }

        /// <summary>
        /// Генерирует URL для маршрута, заданного в приложении.
        /// Параметр routeName позволяет жестко привязать генерацию адреса к конкретному маршруту
        /// </summary>
        /// <param name="routeName">Строка. Имя маршрута</param>
        /// <param name="fields">Структура. Поля маршрута в виде структуры.</param>
        /// <returns>РезультатДействияПеренаправление</returns>
        [ContextMethod("АдресМаршрута")]
        public string RouteUrl(string routeName = null, StructureImpl fields = null)
        {
            string result;
            if (fields != null)
            {
                var values = new Dictionary<string, object>();
                foreach (var kv in fields)
                {
                    values.Add(kv.Key.AsString(), CustomMarshaller.ConvertToCLRObject(kv.Value));
                }

                result = routeName != null ? Url.RouteUrl(routeName, values) : Url.RouteUrl(values);
            }
            else
            {
                if (routeName == null)
                    throw RuntimeException.TooFewArgumentsPassed();

                result = Url.RouteUrl(routeName);
            }

            return result;
        }

        /// <summary>
        /// Генерирует Url для действия в контроллере
        /// </summary>
        /// <param name="action">Имя действия</param>
        /// <param name="fieldsOrController">Имя контроллера строкой или структура полей маршрута.</param>
        /// <returns></returns>
        [ContextMethod("АдресДействия", "ActionUrl")]
        public string ActionUrl(string action, IValue fieldsOrController = null)
        {
            string result;
            if (fieldsOrController != null)
            {
                if (fieldsOrController.DataType == DataType.String)
                {
                    result = Url.Action(action, fieldsOrController.AsString());
                }
                else if (fieldsOrController.GetRawValue() is StructureImpl)
                {
                    var values = new Dictionary<string, object>();
                    foreach (var kv in (StructureImpl)fieldsOrController.GetRawValue())
                    {
                        values.Add(kv.Key.AsString(), CustomMarshaller.ConvertToCLRObject(kv.Value));
                    }

                    result = Url.Action(action, values);
                }
                else
                {
                    throw RuntimeException.InvalidArgumentType(nameof(fieldsOrController));
                }

            }
            else
            {
                result = Url.Action(action);
            }

            return result;
        }

        /// <summary>
        /// Вспомогательный метод генерации ответа в виде представления.
        /// </summary>
        /// <param name="nameOrModel">Имя представления или объект Модели (если используется представление по умолчанию)</param>
        /// <param name="model">Объект модели (произвольный)</param>
        /// <returns>РезультатКомпонентаПредставление.</returns>
        [ContextMethod("Представление")]
        public ViewComponentViewResult View(IValue nameOrModel = null, IValue model = null)
        {
            if (nameOrModel == null && model == null)
            {
                return DefaultViewResult();
            }

            if (model == null)
            {
                if (nameOrModel.DataType == DataType.String)
                {
                    return ViewResultByName(nameOrModel.AsString(), null);
                }
                else
                {
                    return ViewResultByName(null, nameOrModel);
                }
            }

            if (nameOrModel == null)
                return ViewResultByName(null, model);

            return ViewResultByName(nameOrModel.AsString(), model);
        }

        private ViewComponentViewResult ViewResultByName(string viewname, IValue model)
        {
            if (model != null)
                ViewData.Model = model;

            var va = new ViewComponentViewResult()
            {
                ViewName = viewname,
                ViewData = ViewData
            };

            return va;
        }

        private ViewComponentViewResult DefaultViewResult()
        {
            return new ViewComponentViewResult() { ViewData = ViewData };
        }

    }
}
