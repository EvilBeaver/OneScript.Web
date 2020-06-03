using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Экземпляр посредника. Представлен произвольным скриптом в структуре приложения.
    /// </summary>
    [ContextClass("Посредник", "Middleware")]
    [NonController]
    public class ScriptedMiddleware : AutoScriptDrivenObject<ScriptedMiddleware>
    {
        private readonly RequestDelegate _next;
        private HttpContext _context;

        #region Construction

        /// <summary>
        /// Конструктор посредника
        /// </summary>
        /// <param name="next">Следующий обработчик в конвейере</param>
        /// <param name="module">Скомпилированный модуль посредника</param>
        public ScriptedMiddleware(RequestDelegate next, LoadedModule module) : base(module, true)
        {
            _next = next;
            InitOwnData();
        }

        #endregion

        /// <summary>
        /// Основная нагрузка, выполняет метод ОбработкаВызова посредника
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            _context = context;
            var mId = GetScriptMethod("ОбработкаВызова");
            if (mId == -1)
            {
                throw RuntimeException.MethodNotFoundException("ОбработкаВызова");
            }
            HttpRequest = new HttpRequestImpl(_context.Request);
            HttpResponse = new HttpResponseImpl(_context.Response);

            var parameters = new IValue[] { new ArrayImpl() };
            await Task.Run(() => CallScriptMethod(mId, parameters));
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
            var t = Task.Run(() => _next(_context));
            t.Wait();
        }

        // TODO: Костыль вызванный ошибкой https://github.com/EvilBeaver/OneScript/issues/660
        internal static int GetOwnMethodsRelectionOffset()
        {
            return _ownMethods.Count;
        }
    }
}
