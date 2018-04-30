using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneScript.WebHost.Application;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure
{
    public interface IApplicationFactory
    {
        ApplicationInstance CreateApp();
    }

    public class AppStarter : IApplicationFactory, IHostApplication
    {
        private readonly IScriptsProvider _scripts;
        private readonly IApplicationRuntime _webEng;
        private readonly ILogger<ApplicationInstance> _logger;

        private string rootSrc;

        public AppStarter(IScriptsProvider scripts, IApplicationRuntime webEng, IConfigurationRoot config, ILogger<ApplicationInstance> appLog)
        {
            _scripts = scripts;
            _webEng = webEng;
            _logger = appLog;

            var configSection = config?.GetSection("OneScript");
            var libRoot = configSection?["lib.system"];
            
            var mainSrc = configSection?["src.dir"];
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
               
            
            if (mainSrc != null)
            {
                
                rootSrc = mainSrc;
            }
            else
            {
                rootSrc = "";
            }

            
            if (libRoot != null)
            {
                var additionals = configSection.GetSection("lib.additional")?
                    .AsEnumerable()
                    .Where(x=>x.Value != null)
                    .Select(x=>x.Value.Replace("$appBinary", binFolder))
                    .ToArray();

                libRoot = libRoot.Replace("$appBinary", binFolder);
                InitializeDirectiveResolver(_webEng.Engine, _webEng.Environment, libRoot, additionals);
            }
        }

        private void InitializeDirectiveResolver(ScriptingEngine engine, RuntimeEnvironment env, string libRoot, string[] additionals)
        {
            var ignoreDirectiveResolver = new DirectiveIgnorer();

            ignoreDirectiveResolver.Add("Region", "Область");
            ignoreDirectiveResolver.Add("EndRegion", "КонецОбласти");

            var resolversCollection = new DirectiveMultiResolver();
            resolversCollection.Add(ignoreDirectiveResolver);

            var libResolver = new LibraryResolver(engine, env);
            libResolver.LibraryRoot = libRoot;
            if (additionals != null)
                libResolver.SearchDirectories.AddRange(additionals);

            resolversCollection.Add(libResolver);
            engine.DirectiveResolver = resolversCollection;
        }

        public ApplicationInstance CreateApp()
        {
            var codeSrc = _scripts.Get(rootSrc + "/main.os");
            
            _webEng.Environment.InjectObject(new WebGlobalContext(this, codeSrc));
            _webEng.Engine.UpdateContexts();
            
            return ApplicationInstance.Create(codeSrc, _webEng);
        }

        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            switch (status)
            {
                case MessageStatusEnum.WithoutStatus:
                case MessageStatusEnum.Ordinary:
                    _logger.LogDebug(str);
                    break;
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
