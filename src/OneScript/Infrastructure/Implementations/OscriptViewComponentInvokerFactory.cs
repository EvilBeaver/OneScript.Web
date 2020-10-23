using System;
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
