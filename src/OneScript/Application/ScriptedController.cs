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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Главный класс, отвечающий за обработку входящего запроса и генерацию ответа.
    /// </summary>
    [ContextClass("Контроллер")]
    [NonController]
    public class ScriptedController : AutoScriptDrivenObject<ScriptedController>
    {
        private ControllerContext _ctx;
        private IUrlHelper _url;

        private SessionImpl _session;

        private ViewDataDictionaryWrapper _osViewData;
        private ModelStateDictionaryWrapper _modelState;
        
        public ScriptedController(ControllerContext context, LoadedModule module) : base(module, true)
        {
            _ctx = context;
            HttpRequest = new HttpRequestImpl(_ctx.HttpContext.Request);
            HttpResponse = new HttpResponseImpl(_ctx.HttpContext.Response);
            
            if (_ctx.RouteData != null)
            {
                RouteValues = new MapImpl();
                foreach (var routeData in _ctx.RouteData.Values)
                {
                    var rv = RouteValues.AsObject();
                    rv.SetIndexedValue(
                        ValueFactory.Create(routeData.Key),
                        CustomMarshaller.ConvertToIValueSafe(routeData.Value, routeData.Value?.GetType())
                    );
                }
            }
            else
                RouteValues = ValueFactory.Create();

            var typeClr = (Type)context.ActionDescriptor.Properties["type"];
            var type = TypeManager.RegisterType("Контроллер."+typeClr.Name, typeof(ScriptedController));
            DefineType(type);
            InitOwnData();
        }

        public void VoidAction(params object[] parameters)
        {
            var meth = (System.Reflection.MethodInfo)_ctx.ActionDescriptor.Properties["actionMethod"];
            if(parameters == null)
                parameters = new object[0];
            meth.Invoke(this, parameters);
        }

        public IActionResult ResultAction()
        {
            var meth = (System.Reflection.MethodInfo)_ctx.ActionDescriptor.Properties["actionMethod"];
            
            IActionResult result;
            if (meth is ReflectedMethodInfo reflected)
            {
                var res = reflected.InvokeDirect(this, new IValue[0]);

                result = res is IObjectWrapper wrapper
                    ? wrapper.UnderlyingObject as IActionResult
                    : res as IActionResult;
            }
            else
                result = meth.Invoke(this, new object[0]) as IActionResult;

            if(result == null)
                throw new InvalidOperationException("Function must return an IActionResult value");

            return result;
        }

        [ViewDataDictionary]
        public ViewDataDictionary FrameworkViewData
        {
            get { return ViewData?.GetDictionary();}
            set
            {
                ViewData = new ViewDataDictionaryWrapper(value);
            }
        }

        /// <summary>
        /// Входящий запрос HTTP
        /// </summary>
        [ContextProperty("ЗапросHttp")]
        public HttpRequestImpl HttpRequest { get; }

        /// <summary>
        /// Исходящий ответ HTTP
        /// </summary>
        [ContextProperty("ОтветHttp")]
        public HttpResponseImpl HttpResponse { get; }

        /// <summary>
        /// Действующие значения маршрута для текущего вызова.
        /// Тип: Соответствие или Неопределено.
        /// Ключами соответствия являются переменные маршрута.
        /// </summary>
        [ContextProperty("ЗначенияМаршрута")]
        public IValue RouteValues { get; }

        /// <summary>
        /// Данные http-сессии. Механизм сессий использует Cookies для привязки сессии и InMemory хранилище для данных сессии.
        /// </summary>
        [ContextProperty("Сессия")]
        public SessionImpl Session
        {
            get
            {
                if(_session == null)
                    _session = new SessionImpl(_ctx.HttpContext.Session);
                return _session;
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

        [ContextProperty("СостояниеМодели", "ModelState")]
        public ModelStateDictionaryWrapper ModelState =>
            _modelState ?? (_modelState = new ModelStateDictionaryWrapper(_ctx.ModelState));
        
        /// <summary>
        /// Вспомогательный метод генерации ответа в виде представления.
        /// </summary>
        /// <param name="nameOrModel">Имя представления или объект Модели (если используется представление по умолчанию)</param>
        /// <param name="model">Объект модели (произвольный)</param>
        /// <returns>РезультатДействияПредставление.</returns>
        [ContextMethod("Представление")]
        public ViewActionResult View(IValue nameOrModel = null, IValue model = null)
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

        /// <summary>
        /// Вспомогательный метод генерации ответа в виде текстового содержимого
        /// </summary>
        /// <param name="content">Содержимое ответа</param>
        /// <param name="contentType">Кодировка текста ответа</param>
        /// <returns>РезультатДействияСодержимое</returns>
        [ContextMethod("Содержимое")]
        public ContentActionResult Content(string content, string contentType = null)
        {
            var ctResult = new ContentActionResult()
            {
                Content = content,
                ContentType = contentType
            };

            return ctResult;
        }

        /// <summary>
        /// Вспомогательный метод генерации ответа в виде скачиваемого файла.
        /// </summary>
        /// <param name="data">Данные файла (путь или ДвоичныеДанные)</param>
        /// <param name="contentType">Содержимое заголовка Content-type</param>
        /// <param name="downloadFileName">Имя скачиваемого файла</param>
        /// <returns>РезультатДействияФайл</returns>
        [ContextMethod("Файл")]
        public FileActionResult File(IValue data, string contentType = null, string downloadFileName = null)
        {
            FileActionResult fileResult;
            if (data.DataType == DataType.String)
            {
                fileResult = new FileActionResult(data.AsString(), contentType);
            }
            else
            {
                var obj = data.AsObject() as BinaryDataContext;
                if (obj == null)
                    throw RuntimeException.InvalidArgumentType(nameof(data));

                fileResult = new FileActionResult(obj, contentType);
            }

            if (downloadFileName != null)
                fileResult.DownloadFileName = downloadFileName;

            return fileResult;
        }

        /// <summary>
        /// Вспомогательный метод, генерирующий код состояния HTTP
        /// </summary>
        /// <param name="code">Код состояния</param>
        /// <returns>РезультатДействияКодСостояния</returns>
        [ContextMethod("КодСостояния")]
        public StatusCodeActionResult StatusCode(int code)
        {
            return StatusCodeActionResult.Constructor(code);
        }

        /// <summary>
        /// Вспомогательный метод, генерирующий ответ в виде http-редиректа
        /// </summary>
        /// <param name="url">Адрес перенаправления</param>
        /// <param name="permanent">Признак постоянного (permanent) перенаправления.</param>
        /// <returns>РезультатДействияПеренаправление</returns>
        [ContextMethod("Перенаправление")]
        public RedirectActionResult Redirect(string url, bool permanent = false)
        {
            return RedirectActionResult.Create(url, permanent);
        }

        /// <summary>
        /// Вспомогательный метод, генерирующий ответ в виде http-редиректа
        /// </summary>
        /// <param name="action">Имя действия перенаправления</param>
        /// <param name="controller">Контроллер перенаправления</param>
        /// <param name="fields">Дополнительные поля</param>
        /// <param name="permanent">Признак постоянного (permanent) перенаправления.</param>
        /// <returns>РезультатДействияПеренаправление</returns>
        [ContextMethod("ПеренаправлениеНаДействие")]
        public RedirectActionResult RedirectToAction(string action, string controller = null, StructureImpl fields = null, bool permanent = false)
        {
            if(fields == null)
                fields = new StructureImpl();

            if(controller != null)
                fields.Insert("controller", ValueFactory.Create(controller));

            var url = ActionUrl(action, fields);
            if(url == null)
                throw new RuntimeException("Не обнаружен заданный маршрут.");

            return RedirectActionResult.Create(url, permanent);
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
        /// <param name="fieldsOrController">Имя контроллера строкой или структура/соответствие полей маршрута.</param>
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
                else if(fieldsOrController.GetRawValue() is IEnumerable<KeyAndValueImpl>)
                {
                    var values = new Dictionary<string, object>();
                    foreach (var kv in (IEnumerable<KeyAndValueImpl>) fieldsOrController.GetRawValue())
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
        
        public IUrlHelper Url
        {
            get
            {
                if (_url == null)
                {
                    var services = _ctx?.HttpContext?.RequestServices;
                    if (services == null)
                    {
                        return null;
                    }

                    var factory = services.GetRequiredService<IUrlHelperFactory>();
                    _url = factory.GetUrlHelper(_ctx);
                }

                return _url;
            }
        }

        private ViewActionResult ViewResultByName(string viewname, IValue model)
        {
            if(model != null)
                ViewData.Model = model;

            var va = new ViewActionResult()
            {
                ViewName = viewname,
                ViewData = ViewData
            };

            return va;
        }

        private ViewActionResult DefaultViewResult()
        {
            return new ViewActionResult() { ViewData = ViewData };
        }
    }
}
