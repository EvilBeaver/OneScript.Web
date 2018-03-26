using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
    [NonController]
    public class ScriptedController : ScriptDrivenObject
    {
        private ControllerContext _ctx;

        private SessionImpl _session;

        private ViewDataDictionaryWrapper _osViewData;
        private static ContextPropertyMapper<ScriptedController> _ownProperties = new ContextPropertyMapper<ScriptedController>();
        private static ContextMethodsMapper<ScriptedController> _ownMethods = new ContextMethodsMapper<ScriptedController>();
        
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
                        CustomMarshaller.ConvertToIValueSafe(routeData.Value, routeData.Value.GetType())
                    );
                }
            }
            else
                RouteValues = ValueFactory.Create();

            var typeClr = (Type)context.ActionDescriptor.Properties["type"];
            var type = TypeManager.RegisterType(typeClr.Name, typeof(ScriptedController));
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

        public IActionResult ResultAction(params object[] parameters)
        {
            var meth = (System.Reflection.MethodInfo)_ctx.ActionDescriptor.Properties["actionMethod"];
            if (parameters == null)
                parameters = new object[0];
            var result = meth.Invoke(this, parameters) as IActionResult;
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

        #region SDO Methods

        protected override string GetOwnPropName(int index)
        {
            return _ownProperties.GetProperty(index).Name;
        }

        protected override int GetOwnVariableCount()
        {
            return _ownProperties.Count;
        }

        protected override int GetOwnMethodCount()
        {
            return _ownMethods.Count;
        }

        protected override void UpdateState()
        {
        }

        protected override int FindOwnProperty(string name)
        {
            return _ownProperties.FindProperty(name);
        }

        protected override bool IsOwnPropReadable(int index)
        {
            return _ownProperties.GetProperty(index).CanRead;
        }

        protected override bool IsOwnPropWritable(int index)
        {
            return _ownProperties.GetProperty(index).CanWrite;
        }

        protected override IValue GetOwnPropValue(int index)
        {
            return _ownProperties.GetProperty(index).Getter(this);
        }

        protected override void SetOwnPropValue(int index, IValue val)
        {
            _ownProperties.GetProperty(index).Setter(this, val);
        }

        protected override int FindOwnMethod(string name)
        {
            try
            {
                int idx = _ownMethods.FindMethod(name);
                return idx;
            }
            catch (RuntimeException)
            {
                return -1;
            }
        }

        protected override MethodInfo GetOwnMethod(int index)
        {
            return _ownMethods.GetMethodInfo(index);
        }

        protected override IValue CallOwnFunction(int index, IValue[] arguments)
        {
            return _ownMethods.GetMethod(index)(this, arguments);
        }

        protected override void CallOwnProcedure(int index, IValue[] arguments)
        {
            _ownMethods.GetMethod(index)(this, arguments);
        }

        #endregion

        public static ModuleImage CompileModule(CompilerService compiler, ICodeSource src)
        {
            for (int i = 0; i < _ownProperties.Count; i++)
            {
                var currentProp = _ownProperties.GetProperty(i);
                compiler.DefineVariable(currentProp.Name, currentProp.Alias, SymbolType.ContextProperty);
            }

            for (int i = 0; i < _ownMethods.Count; i++)
            {
                compiler.DefineMethod(_ownMethods.GetMethodInfo(i));
            }

            return compiler.Compile(src);
        }

        // TODO: Костыль вызванный ошибкой https://github.com/EvilBeaver/OneScript/issues/660
        internal static int GetOwnMethodsRelectionOffset()
        {
            return _ownMethods.Count;
        }
    }
}
