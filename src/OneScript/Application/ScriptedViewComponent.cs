using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [NonViewComponent]
    public class ScriptedViewComponent : ScriptDrivenObject
    {
        public const string InvokeMethodNameRu = "ОбработкаВызова";
        public const string InvokeMethodNameEn = "CallProcessing";

        private Func<object> _invoker;

        private IUrlHelper _url;
        private SessionImpl _session;
        private ViewComponentContext _ctx;
        private ViewDataDictionaryWrapper _osViewData;

        private const int THISOBJ_VARIABLE_INDEX = 0;
        private const string THISOBJ_EN = "ThisObject";
        private const string THISOBJ_RU = "ЭтотОбъект";
        private const int PRIVATE_PROPS_OFFSET = 1;

        private static ContextPropertyMapper<ScriptedViewComponent> _ownProperties = new ContextPropertyMapper<ScriptedViewComponent>();
        private static ContextMethodsMapper<ScriptedViewComponent> _ownMethods = new ContextMethodsMapper<ScriptedViewComponent>();

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

            object Invoker()
            {
                var result = CallScriptMethod(methId, new IValue[0]);
                return CustomMarshaller.ConvertToCLRObject(result);
            }

            _invoker = Invoker;
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

        public IViewComponentResult Invoke()
        {
            return _invoker() as IViewComponentResult;
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
                    throw RuntimeException.TooLittleArgumentsPassed();

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


        #region SDO Methods

        protected override string GetOwnPropName(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return THISOBJ_RU;

            return _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).Name;
        }

        protected override int GetOwnVariableCount()
        {
            return _ownProperties.Count + PRIVATE_PROPS_OFFSET;
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
            if (string.Compare(name, THISOBJ_RU, StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(name, THISOBJ_EN, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return THISOBJ_VARIABLE_INDEX;
            }

            return _ownProperties.FindProperty(name) + PRIVATE_PROPS_OFFSET;
        }

        public override int FindProperty(string name)
        {
            return base.FindProperty(name);
        }

        protected override bool IsOwnPropReadable(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return true;

            return _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).CanRead;
        }

        protected override bool IsOwnPropWritable(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return false;

            return _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).CanWrite;
        }

        protected override IValue GetOwnPropValue(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return this;

            return _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).Getter(this);
        }

        protected override void SetOwnPropValue(int index, IValue val)
        {
            _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).Setter(this, val);
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
            compiler.DefineVariable(THISOBJ_RU, THISOBJ_EN, SymbolType.ContextProperty);
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

    }
}
