using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using OneScript.WebHost.Infobase;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Database
{
    /// <summary>
    /// Обертка соединения с информационной базой
    /// </summary>
    [ContextClass("СоединениеИнформационнойБазы", "InfobaseConnection")]
    public class InfobaseContext : AutoContext<InfobaseContext>
    {
        private readonly IServiceProvider _services;

        public InfobaseContext(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Создает новый запрос к базе данных
        /// </summary>
        /// <returns>Запрос.</returns>
        [ContextMethod("НовыйЗапрос")]
        public DbQueryContext NewQuery()
        {
            return new DbQueryContext(_services.GetRequiredService<ApplicationDbContext>());
        }
    }
}
