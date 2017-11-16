using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneScript.WebHost.Infrastructure;

namespace OneScript.WebHost.Application
{
    interface IApplicationFactory
    {
        ApplicationInstance CreateApp();
    }

    class AppStarter : IApplicationFactory
    {
        private IApplicationModulesLocator _modFactory;
        private WebApplicationEngine _webEng;

        public AppStarter(IApplicationModulesLocator modFactory, WebApplicationEngine webEng)
        {
            _modFactory = modFactory;
            _webEng = webEng;
        }

        public ApplicationInstance CreateApp()
        {
            var codeSrc = _modFactory.SourceProvider.Get("/main.os");
            var module = _modFactory.PrepareModule(codeSrc);
            var app = new ApplicationInstance(module);
            _webEng.Engine.InitializeSDO(app);

            return app;
        }
    }
    
}
