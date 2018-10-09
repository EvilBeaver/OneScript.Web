using System;
using Microsoft.Extensions.DependencyInjection;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Database
{
    /// <summary>
    /// Обертка соединения с информационной базой
    /// </summary>
    [ContextClass("СоединениеИнформационнойБазы", "InfobaseConnection")]
    public class InfobaseContext : AutoContext<InfobaseContext>
    {
        public InfobaseContext()
        {
        }

        [ThreadStatic]
        private static ApplicationDbContext _dbContext;

        public ApplicationDbContext DbContext
        {
            get => _dbContext;
            set => _dbContext = value;
        }

        /// <summary>
        /// Создает новый запрос к базе данных
        /// </summary>
        /// <returns>Запрос.</returns>
        [ContextMethod("НовыйЗапрос")]
        public DbQueryContext NewQuery()
        {
            return new DbQueryContext(DbContext);
        }
    }
}
