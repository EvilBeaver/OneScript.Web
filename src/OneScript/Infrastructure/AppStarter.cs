﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using OneScript.WebHost.Application;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace OneScript.WebHost.Infrastructure
{
    public class AppStarter : IApplicationFactory, IHostApplication
    {
        private readonly IFileProvider _scripts;
        private readonly IApplicationRuntime _webEng;
        private readonly ILogger<ApplicationInstance> _logger;

        public AppStarter(IFileProvider scripts, IApplicationRuntime webEng, IConfiguration config, ILogger<ApplicationInstance> appLog)
        {
            _scripts = scripts;
            _webEng = webEng;
            _logger = appLog;

            var configSection = config?.GetSection("OneScript");
            var libRoot = configSection?["lib.system"];
            if (libRoot != null)
            {
                var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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

        public ApplicationInstance CreateApp(IServiceCollection services)
        {
            var codeSrc = new FileInfoCodeSource(_scripts.GetFileInfo("main.os"));
            _webEng.Environment.InjectObject(new WebGlobalContext(this, codeSrc));
            _webEng.Engine.UpdateContexts();
            
            return ApplicationInstance.Create(codeSrc, _webEng, services);
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