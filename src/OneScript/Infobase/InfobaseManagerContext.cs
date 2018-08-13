using System;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Microsoft.Data.Sqlite;
using OScriptSql;

namespace OneScript.WebHost.Infobase
{
    [ContextClass("МенеджерИнформационнойБазы", "InfoBaseManager")]
    public class InfobaseManagerContext : AutoContext<InfobaseManagerContext>
    {
        private readonly IServiceProvider _services;
        private SqliteConnection _connection;

        private int _dbType;
        private string _dbName;
        private string _connectionString;
        private string _lastErrorMessage;

        public InfobaseManagerContext(IServiceProvider services)

        {
            _dbType = 0;
            _dbName = "";
            _connectionString = "";
            _lastErrorMessage = "";

        }

        public override string ToString()
        {
            return "Соединение";
        }

        /// <summary>
        /// Типы поддерживаемых СУБД
        /// </summary>
        /// <value>ТипСУБД</value>
        [ContextProperty("ТипыСУБД", "DBTypes")]
        public IValue DbTypes
        {
            get
            {
                var dtype = new EnumDBType();
                return dtype;
            }
        }

        /// <summary>
        /// Тип подключенной СУБД
        /// </summary>
        /// <value>ТипСУБД</value>
        [ContextProperty("ТипСУБД", "DBType")]
        public int DbType
        {
            get
            {
                return _dbType;
            }

            set
            {
                _dbType = value;
            }
        }

        /// <summary>
        /// Имя базы, в случае с SQLITE - путь к базе
        /// </summary>
        /// <value>Строка</value>
        [ContextProperty("ИмяБазы", "DbName")]
        public string DbName
        {
            get
            {
                return _dbName;
            }

            set
            {
                _dbName = value;
            }
        }

        /// <summary>
        /// Статус соединения с БД
        /// </summary>
        /// <value>ConnectionState</value>
        [ContextProperty("Открыто", "IsOpen")]
        public string IsOpen
        {
            get
            {
                return Connection.State.ToString();
            }
        }

        /// <summary>
        /// Текст последней ошибки
        /// </summary>
        /// <value>Строка</value>
        [ContextProperty("ПоследнееСообщениеОбОшибке", "LastErrorMessage")]
        public string LastErrorMessage
        {
            get
            {
                return _lastErrorMessage;
            }
        }

        public SqliteConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        /// <summary>
        /// Подготовленная строка соединения. В случае sqlite аналог ИмяБазы
        /// </summary>
        /// <value>Строка</value>
        [ContextProperty("СтрокаСоединения", "ConnectionString")]
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }

            set
            {
                _connectionString = value;
            }
        }

        /// <summary>
        /// Открыть соединение с БД
        /// </summary>
        /// <returns>Булево</returns>
        [ContextMethod("Открыть", "Open")]
        public bool Open()
        {
            if (DbType == (new EnumDBType()).sqlite)
            {
                if (ConnectionString == string.Empty && DbName != string.Empty)
                    ConnectionString = string.Format("Data Source={0};", DbName);

                _connection = new SqliteConnection(ConnectionString);

                return OpenConnection();
            }
           
            return false;
        }

        private bool OpenConnection()
        {
            try
            {
                _connection.Open();
                _lastErrorMessage = "";
                return true;
            }
            catch (Exception e)
            {
                _lastErrorMessage = e.Message;
                return false;
            }
        }

        /// <summary>
        /// Закрыть соединение с БД
        /// </summary>
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            _connection.Close();
            _connection.ConnectionString = "";
            _connection.Dispose();
            _connection = null;
        }

        /// <summary>
        /// Выполняет запрос на модификацию к базе данных. 
        /// </summary>
        /// <returns>Число - Число обработанных строк.</returns>
        [ContextMethod("ВыполнитьКоманду")]
        public int RunСommand(string srt)
        {

            if (DbType == (new EnumDBType()).sqlite)
            {
                SqliteCommand cmd = new SqliteCommand(srt, _connection);
                return cmd.ExecuteNonQuery();
            }

            return 0;
        }

        /// <summary>
        /// Создать запрос с установленным соединением
        /// </summary>
        /// <returns>Запрос - Запрос с установленным соединением</returns>
        [ContextMethod("СоздатьЗапрос", "CreateQuery")]
        public Query CreateQuery()
        {
            var query = new Query();
            query.SetConnection(this);
            return query;
        }
    }
}