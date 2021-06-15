/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.HostedScript.Library.ValueTable;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Data.Common;

namespace OneScript.WebHost.Database
{
    /// <summary>
    /// Содержит результат выполнения запроса. Предназначен для хранения и обработки полученных данных.
    /// </summary>
    [ContextClass("РезультатЗапроса", "QueryResult")]
    public class QueryResult : AutoContext<QueryResult>
    {
        ValueTable _resultTable = new ValueTable();

        public QueryResult(DbDataReader reader)
        {
            for (int ColIdx = 0; ColIdx < reader.FieldCount; ColIdx++)
            {
                _resultTable.Columns.Add(reader.GetName(ColIdx));
            }

            using (reader)
            {
                foreach (DbDataRecord record in reader)
                {
                    ValueTableRow row = _resultTable.Add();

                    for (int ColIdx = 0; ColIdx < reader.FieldCount; ColIdx++)
                    {
                        if (record.IsDBNull(ColIdx))
                        {
                            row.Set(ColIdx, ValueFactory.Create());
                            continue;
                        }

                        var fieldType = record.GetFieldType(ColIdx);
                        if (fieldType == typeof(Int32))
                        {
                            row.Set(ColIdx, ValueFactory.Create((int)record.GetValue(ColIdx)));
                        }
                        else if (fieldType == typeof(Int64))
                        {
                            row.Set(ColIdx, ValueFactory.Create(record.GetInt64(ColIdx)));
                        }
                        else if (fieldType == typeof(Boolean))
                        {
                            row.Set(ColIdx, ValueFactory.Create(record.GetBoolean(ColIdx)));
                        }
                        else if (fieldType == typeof(UInt64))
                        {
                            row.Set(ColIdx, ValueFactory.Create(record.GetValue(ColIdx).ToString()));
                        }
                        else if (fieldType == typeof(System.Double))
                        {
                            double val = record.GetDouble(ColIdx);
                            row.Set(ColIdx, ValueFactory.Create(val.ToString()));
                        }
                        else if (fieldType == typeof(Single))
                        {
                            float val = record.GetFloat(ColIdx);
                            row.Set(ColIdx, ValueFactory.Create(val.ToString()));
                        }
                        else if (fieldType == typeof(Decimal))
                        {
                            row.Set(ColIdx, ValueFactory.Create(record.GetDecimal(ColIdx)));
                        }
                        else if (fieldType == typeof(System.DateTime)) 
                        {
                            row.Set(ColIdx, ValueFactory.Create(record.GetDateTime(ColIdx)));
                        }
                        else if (fieldType == typeof(System.String))
                        {
                            row.Set(ColIdx, ValueFactory.Create(record.GetString(ColIdx)));
                        }
                        else if (fieldType == typeof(System.Byte[]))
                        {
                            var data = (byte[])record[ColIdx];
                            var newData = new BinaryDataContext(data);
                            row.Set(ColIdx, ValueFactory.Create(newData));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Создает таблицу значений и копирует в нее все записи набора.
        /// </summary>
        /// <returns>ТаблицаЗначений</returns>
        [ContextMethod("Выгрузить", "Unload")]
        public ValueTable Unload()
        {
            return _resultTable;
        }

        /// <summary>
        /// Возвращает Истина, если в результате запроса нет ни одной записи.
        /// </summary>
        /// <returns>Булево</returns>
        [ContextMethod("Пустой", "IsEmpty")]
        public Boolean IsEmpty()
        {
            return _resultTable.Count() == 0;
        }
    }
}
