using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneScript.WebHost.Infrastructure;

namespace OneScript.WebHost.Application
{
    public interface IApplicationFactory
    {
        ApplicationInstance CreateApp();
    }

    class AppStarter : IApplicationFactory
    {
        private IScriptsProvider _scripts;
        private IApplicationRuntime _webEng;

        public AppStarter(IScriptsProvider scripts, IApplicationRuntime webEng)
        {
            _scripts = scripts;
            _webEng = webEng;
        }

        public ApplicationInstance CreateApp()
        {
            var codeSrc = _scripts.Get("/main.os");
            return ApplicationInstance.Create(codeSrc, _webEng);
        }
    }
    
}
