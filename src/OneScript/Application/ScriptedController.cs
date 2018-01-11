using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneScript.WebHost.Application;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    [NonController]
    public class ScriptedController : ScriptDrivenObject
    {
        private ControllerContext _ctx;
        private static ContextPropertyMapper<ScriptedController> _ownProperties = new ContextPropertyMapper<ScriptedController>();
        private static ContextMethodsMapper<ScriptedController> _ownMethods = new ContextMethodsMapper<ScriptedController>();
        
        public ScriptedController(ControllerContext context, LoadedModuleHandle module) : base(module, true)
        {
            _ctx = context;
            HttpRequest = new HttpRequestImpl(_ctx.HttpContext.Request);
            HttpResponse = new HttpResponseImpl(_ctx.HttpContext.Response);
            Session = new SessionImpl(_ctx.HttpContext.Session);

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

        [ContextProperty("ЗапросHttp")]
        public HttpRequestImpl HttpRequest { get; }

        [ContextProperty("ОтветHttp")]
        public HttpResponseImpl HttpResponse { get; }

        [ContextProperty("ЗначенияМаршрута")]
        public IValue RouteValues { get; }

        [ContextProperty("Сессия")]
        public SessionImpl Session { get; }

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
            return 0;
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
            return _ownMethods.FindMethod(name);
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

        public static ScriptModuleHandle CompileModule(CompilerService compiler, ICodeSource src)
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

            return compiler.CreateModule(src);
        }
    }
}
