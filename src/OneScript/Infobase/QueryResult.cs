
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.HostedScript.Library.ValueTable;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Data.Common;

namespace OScriptSql
{
    /// <summary>
    /// Содержит результат выполнения запроса. Предназначен для хранения и обработки полученных данных.
    /// </summary>
    [ContextClass("РезультатЗапроса", "QueryResult")]
    class QueryResult : AutoContext<QueryResult>
    {
        private DbDataReader _reader;

        public QueryResult()
        {
        }

        public QueryResult(DbDataReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Создает таблицу значений и копирует в нее все записи набора.
        /// </summary>
        /// <returns>ТаблицаЗначений</returns>
        [ContextMethod("Выгрузить", "Unload")]
        public ValueTable Unload()
        {

            ValueTable resultTable = new ValueTable();

            for (int ColIdx = 0; ColIdx < _reader.FieldCount; ColIdx++)
            {
                resultTable.Columns.Add(_reader.GetName(ColIdx));
            }

            foreach (DbDataRecord record in _reader)
            {
                ValueTableRow row = resultTable.Add();

                for (int ColIdx = 0; ColIdx < _reader.FieldCount; ColIdx++)
                {
                    if (record.IsDBNull(ColIdx))
                    {
                        row.Set(ColIdx, ValueFactory.Create());
                        continue;
                    }

                    //Console.WriteLine("queryresult-col-type:" + record.GetFieldType(ColIdx).ToString() + "::" + record.GetDataTypeName(ColIdx));

                    if (record.GetFieldType(ColIdx) == typeof(Int32))
                    {
                        row.Set(ColIdx, ValueFactory.Create((int)record.GetValue(ColIdx)));
                    }
                    if (record.GetFieldType(ColIdx) == typeof(Int64))
                    {
                        row.Set(ColIdx, ValueFactory.Create(record.GetInt64(ColIdx)));
                    }
                    if (record.GetFieldType(ColIdx) == typeof(Boolean))
                    {
                        row.Set(ColIdx, ValueFactory.Create(record.GetBoolean(ColIdx)));
                    }
                    if (record.GetFieldType(ColIdx) == typeof(UInt64))
                    {
                        row.Set(ColIdx, ValueFactory.Create(record.GetValue(ColIdx).ToString()));
                    }

                    if (record.GetFieldType(ColIdx).ToString() == "System.Double")
                    {
                        double val = record.GetDouble(ColIdx);
                        row.Set(ColIdx, ValueFactory.Create(val.ToString()));
                    }
                    if (record.GetFieldType(ColIdx) == typeof(Single))
                    {
                        float val = record.GetFloat(ColIdx);
                        row.Set(ColIdx, ValueFactory.Create(val.ToString()));
                    }
                    if (record.GetFieldType(ColIdx) == typeof(Decimal))
                    {
                        row.Set(ColIdx, ValueFactory.Create(record.GetDecimal(ColIdx)));
                    }
                    if (record.GetFieldType(ColIdx).ToString() == "System.String")
                    {
                        row.Set(ColIdx, ValueFactory.Create(record.GetString(ColIdx)));
                    }
                    if (record.GetFieldType(ColIdx).ToString() == "System.DateTime")
                    {
                        row.Set(ColIdx, ValueFactory.Create(record.GetDateTime(ColIdx)));
                    }
                    if (record.GetFieldType(ColIdx).ToString() == "System.Byte[]")
                    {
                        var data = (byte[])record[ColIdx];
                        var newData = new BinaryDataContext(data);
                        row.Set(ColIdx, ValueFactory.Create(newData));
                    }
                }
            }
            _reader.Close();
            return resultTable;
        }
    }
}