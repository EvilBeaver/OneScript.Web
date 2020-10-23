/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OscriptViewComponentInvokerFactory : IViewComponentInvokerFactory
    {
        private readonly IViewComponentFactory _viewComponentFactory;
        /*private readonly ILogger _logger;*/

        public OscriptViewComponentInvokerFactory(
            IViewComponentFactory viewComponentFactory 
            /*ILoggerFactory loggerFactory*/)
        {
            _viewComponentFactory = viewComponentFactory;
            //_logger = loggerFactory.CreateLogger<OscriptViewComponentInvoker>();
        }

        public IViewComponentInvoker CreateInstance(ViewComponentContext context)
        {
            return new OscriptViewComponentInvoker(_viewComponentFactory);
        }
    }
}
