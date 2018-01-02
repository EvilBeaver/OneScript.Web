using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OneScript.WebHost.Application;
using ScriptEngine;

namespace OneScript.WebHost.Infrastructure
{
    public interface IApplicationFactory
    {
        ApplicationInstance CreateApp();
    }

    class AppStarter : IApplicationFactory
    {
        private IScriptsProvider _scripts;
        private IApplicationRuntime _webEng;

        public AppStarter(IScriptsProvider scripts, IApplicationRuntime webEng, IConfigurationRoot config)
        {
            _scripts = scripts;
            _webEng = webEng;

            var configSection = config.GetSection("OneScript");
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

                _webEng.Engine.DirectiveResolver = new LibraryResolverAdHoc(webEng, libRoot, additionals);
            }
        }

        public ApplicationInstance CreateApp()
        {
            var codeSrc = _scripts.Get("/main.os");
            return ApplicationInstance.Create(codeSrc, _webEng);
        }
    }
    
}
