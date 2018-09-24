using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.HostedScript.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using System.Data.Common;
using OneScript.WebHost.Infobase;

namespace OScriptSql
{
    interface IOScriptQuery : IValue
    {
        // props
        StructureImpl Parameters { get; }
        string Text { get; set; }

        // methods
        IValue Execute();
        void SetParameter(string ParametrName, IValue ParametrValue);

        // my methods
        void SetConnection(InfobaseManagerContext connector);


    }
}
