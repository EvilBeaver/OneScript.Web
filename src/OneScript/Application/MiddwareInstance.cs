using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Экземпляр посредника. Представлен модулем посредника в подпапке middleware проекта
    /// </summary>
    [ContextClass("Посредник")]
    public class MiddlewareInstance
    {
        private readonly RequestDelegate _next;
        private HttpContext _context;

        /// <summary>
        /// Конструктор посредника
        /// </summary>
        /// <param name="next"></param>
        public MiddlewareInstance(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            this._context = context;
            
        }

        /// <summary>
        /// 
        /// </summary>
        [ContextMethod("Следующий")]
        public void InvokeNext()
        {
            _next.Invoke(this._context);
        }
    }
}
