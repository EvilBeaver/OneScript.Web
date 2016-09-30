using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScriptEngine.HostedScript;

namespace OneScript.Mvc
{
    public class OneScript
    {
        private static HostedScriptEngine _engine;
        public static void Initialize()
        {
            _engine = new HostedScriptEngine();
        }
    }
}