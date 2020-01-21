using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Экземпляр посредника. Представлен модулем посредника в подпапке middleware проекта
    /// </summary>
    [ContextClass("Посредник")]
    public class MiddlewareInstance : AutoScriptDrivenObject<ScriptedController>
    {
        private readonly RequestDelegate _next;
        private HttpContext _context;

        /// <summary>
        /// Конструктор посредника
        /// </summary>
        /// <param name="next"></param>
        public MiddlewareInstance(RequestDelegate next, LoadedModule module) : base(module, true)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            this._context = context;
            var mId = GetScriptMethod("ОбработкаВызова");
            if (mId == -1)
            {
                return;
            }
            HttpRequest = new HttpRequestImpl(_context.Request);
            HttpResponse = new HttpResponseImpl(_context.Response);

            var boolValue = ValueFactory.Create(standardHandling);
            var boolReference = Variable.Create(boolValue, "");
            var parameters = new IValue[] { new ArrayImpl(), boolReference };
            CallScriptMethod(mId, parameters);

            var arr = parameters[0].AsObject() as ArrayImpl;
            if (arr == null)
                throw RuntimeException.InvalidArgumentType();

            files = arr.Select(x => x.AsString());
            standardHandling = parameters[1].AsBoolean();

        }

        /// <summary>
        /// Входящий запрос HTTP
        /// </summary>
        [ContextProperty("ЗапросHttp")]
        public HttpRequestImpl HttpRequest { get; set; }

        /// <summary>
        /// Исходящий ответ HTTP
        /// </summary>
        [ContextProperty("ОтветHttp")]
        public HttpResponseImpl HttpResponse { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ContextMethod("ПродолжитьОбработку")]
        public void InvokeNext()
        {
            _next.Invoke(this._context);
        }
    }
}
