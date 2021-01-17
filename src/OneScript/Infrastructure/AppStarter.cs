/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using OneScript.StandardLibrary;
using OneScript.WebHost.Application;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure
{
    public class AppStarter : IApplicationFactory, IHostApplication
    {
        private readonly IFileProvider _scripts;
        private readonly IApplicationRuntime _webEng;
        private readonly ILogger<ApplicationInstance> _logger;

        public AppStarter(IFileProvider scripts, IApplicationRuntime webEng, ILogger<ApplicationInstance> appLog)
        {
            _scripts = scripts;
            _webEng = webEng;
            _logger = appLog;
        }

        public ApplicationInstance CreateApp()
        {
            var codeSrc = new FileInfoCodeSource(_scripts.GetFileInfo("main.os"));
            //_webEng.Environment.InjectObject(new WebGlobalContext(this, codeSrc, _webEng));

            _webEng.Environment.InjectObject(new SystemGlobalContext
            {
                CodeSource = codeSrc,
                ApplicationHost = this
            });
            
            _webEng.Environment.InjectObject(new DynamicLoadingFunctions(_webEng.Engine));
            
            var templateFactory = new DefaultTemplatesFactory();
            
            var storage = new TemplateStorage(templateFactory);
            _webEng.Environment.InjectObject(storage);
            _webEng.Engine.UpdateContexts();
            GlobalsManager.RegisterInstance(storage);
            
            return ApplicationInstance.Create(codeSrc, _webEng);
        }

        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.WithoutStatus)
        {
            switch (status)
            {
                case MessageStatusEnum.WithoutStatus:
                    _logger.LogDebug(str);
                    break;
                case MessageStatusEnum.Ordinary:
                case MessageStatusEnum.Information:
                    _logger.LogInformation(str);
                    break;
                case MessageStatusEnum.Important:
                case MessageStatusEnum.VeryImportant:
                    _logger.LogError(str);
                    break;
                case MessageStatusEnum.Attention:
                    _logger.LogWarning(str);
                    break;
            }
        }

        public void ShowExceptionInfo(Exception exc)
        {
            throw new NotImplementedException();
        }

        public bool InputString(out string result, int maxLen)
        {
            throw new NotImplementedException();
        }

        public string[] GetCommandLineArguments()
        {
            throw new NotImplementedException();
        }
    }
}
