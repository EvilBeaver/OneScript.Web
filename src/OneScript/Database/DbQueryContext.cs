/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Database
{
    /// <summary>
    /// Запрос к информационной базе. Используется только если создана конфигурация соединения с ИБ
    /// </summary>
    [ContextClass("Запрос", "Query")]
    public class DbQueryContext : AutoContext<DbQueryContext>
    {
        private ApplicationDbContext _dbContext;

        public DbQueryContext(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [ContextProperty("Параметры", "Parameters")]
        public StructureImpl Parameters { get; } = new StructureImpl();

        /// <summary>
        /// Содержит исходный текст выполняемого запроса.
        /// </summary>
        /// <value>Строка</value>
        [ContextProperty("Текст", "Text")]
        public string Text { get; set; } = "";


        /// <summary>
        /// Выполняет запрос к базе данных. 
        /// </summary>
        /// <returns>РезультатЗапроса</returns>
        [ContextMethod("Выполнить", "Execute")]
        public QueryResult Execute()
        {
            /*using (*/
            var connection = _dbContext.Database.GetDbConnection();/*)*/
            using (var command = connection.CreateCommand())
            {
                if (ConnectionIsOpen(connection))
                {
                    connection.Open();
                }
                command.CommandText = Text;
                SetDbCommandParameters(command);
                var reader = command.ExecuteReader();
                return new QueryResult(reader);
            }
        }

        /// <summary>
        /// Выполняет запрос на модификацию к базе данных. 
        /// </summary>
        /// <returns>Число - Число обработанных строк.</returns>
        [ContextMethod("ВыполнитьКоманду", "ExecuteCommand")]
        public int ExecuteCommand()
        {
            /*using (*/
            var connection = _dbContext.Database.GetDbConnection();/*)*/
            using (var command = connection.CreateCommand())
            {
                if (ConnectionIsOpen(connection))
                {
                    connection.Open();
                }
                command.CommandText = Text;
                SetDbCommandParameters(command);

                SetDbCommandParameters(command);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Устанавливает параметр запроса. Параметры доступны для обращения в тексте запроса. 
        /// С помощью этого метода можно передавать переменные в запрос, например, для использования в условиях запроса.
        /// ВАЖНО: В запросе имя параметра указывается с использованием '@'.
        /// </summary>
        /// <example>
        /// Запрос.Текст = "select * from mytable where category_id = @category_id";
        /// Запрос.УстановитьПараметр("category_id", 1);
        /// </example>
        /// <param name="parametrName">Строка - Имя параметра</param>
        /// <param name="parametrValue">Произвольный - Значение параметра</param>
        [ContextMethod("УстановитьПараметр", "SetParameter")]
        public void SetParameter(string parametrName, IValue parametrValue)
        {
            Parameters.Insert(parametrName, parametrValue);
        }

        private bool ConnectionIsOpen(DbConnection connection)
        {
            return connection.State != System.Data.ConnectionState.Open;
        }

        private void SetDbCommandParameters(DbCommand command)
        {
            DbParameter param = null;
            command.Parameters.Clear();
            foreach (KeyAndValueImpl prm in Parameters)
            {
                var paramVal = prm.Value;
                var paramKey = prm.Key.AsString();

                if (paramVal.DataType == DataType.String)
                {
                    param = command.CreateParameter();
                    param.ParameterName = "@" + paramKey;
                    param.Value = paramVal.AsString();
                }
                else if (paramVal.DataType == DataType.Number)
                {
                    param = command.CreateParameter();
                    param.ParameterName = "@" + paramKey;
                    param.Value = paramVal.AsNumber();
                }
                else if (paramVal.DataType == DataType.Date)
                {
                    param = command.CreateParameter();
                    param.ParameterName = "@" + paramKey;
                    param.Value = paramVal.AsDate();
                }
                else if (paramVal.DataType == DataType.Boolean)
                {
                    param = command.CreateParameter();
                    param.ParameterName = "@" + paramKey;
                    param.Value = paramVal.AsBoolean();
                }

                command.Parameters.Add(param);
            }

        }
    }
}
