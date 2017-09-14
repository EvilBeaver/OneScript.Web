using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace OneScript.WebHost.Infrastructure
{
    public class WebApplicationHost : IHostApplication
    {
        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            throw new NotImplementedException();
        }

        public string[] GetCommandLineArguments()
        {
            throw new NotImplementedException();
        }

        public bool InputString(out string result, int maxLen)
        {
            throw new NotImplementedException();
        }

        public void ShowExceptionInfo(Exception exc)
        {
            // в веб-приложении кидаем эксепшн на уровень сервера
            throw exc;
        }
    }
}
