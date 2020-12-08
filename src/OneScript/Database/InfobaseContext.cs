/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
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
