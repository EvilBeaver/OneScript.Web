using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using OneScript.WebHost.Application;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OscriptViewComponentInvoker : IViewComponentInvoker
    {
        private readonly IViewComponentFactory _viewComponentFactory;

        public OscriptViewComponentInvoker(IViewComponentFactory factory)
        {
            _viewComponentFactory = factory;
        }

        public Task InvokeAsync(ViewComponentContext context)
        {
            var uncastedcomponent = _viewComponentFactory.CreateViewComponent(context);
            
            Debug.Assert(uncastedcomponent is ScriptedViewComponent);
            var component = (ScriptedViewComponent) uncastedcomponent;

            var result = component.Invoke();
            _viewComponentFactory.ReleaseViewComponent(context, component);

            return result.ExecuteResultAsync(context.ViewContext);
        }
    }
}