using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OScriptSql
{
    /// <summary>
    /// Тип поддерживаемой СУБД
    /// </summary>
    [ContextClass("ТипСУБД", "DBType")]
    public class EnumDBType : AutoContext<EnumDBType>
    {
        [ContextProperty("sqlite", "sqlite")]
        public int sqlite
        {
            get { return 0; }
        }

    }
}
