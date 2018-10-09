using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
            var obj = component.Invoke(context.Arguments);
            _viewComponentFactory.ReleaseViewComponent(context, component);

            var result = CoerceToViewComponentResult(obj);
            return result.ExecuteAsync(context);
        }

        private static IViewComponentResult CoerceToViewComponentResult(object value)
        {
            if (value == null)
                throw new InvalidOperationException("NULL is not a valid result for ViewComponent");

            if (value is IViewComponentResult viewComponentResult)
                return viewComponentResult;

            if (value is string content)
                return (IViewComponentResult)new ContentViewComponentResult(content);


            throw new InvalidOperationException($"Can't use type {value.GetType()} as the result of view component");
        }
    }
}