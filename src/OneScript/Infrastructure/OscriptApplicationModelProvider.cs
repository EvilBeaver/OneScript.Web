using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OneScript.WebHost.Infrastructure
{
    public class OscriptApplicationModelProvider : IApplicationModelProvider
    {
        private readonly IApplicationModulesLocator _fw;
        public OscriptApplicationModelProvider(IApplicationModulesLocator framework)
        {
            _fw = framework;
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            var attrList = new List<string>();
            var sources = _fw.SourceProvider.EnumerateFiles("/controllers");
            var reflector = new TypeReflectionEngine();
            foreach (var virtualPath in sources)
            {
                var codeSrc = _fw.SourceProvider.Get(virtualPath);
                var module = _fw.PrepareModule(codeSrc);
                var baseFileName = System.IO.Path.GetFileNameWithoutExtension(codeSrc.SourceDescription);
                var type = reflector.Reflect(module, baseFileName);
                var cm = new ControllerModel(type.GetTypeInfo(), attrList.AsReadOnly());
                cm.ControllerName = type.Name;
                cm.Properties.Add("module", module);
                FillActions(cm, type);

                context.Result.Controllers.Add(cm);
            }
        }

        private void FillActions(ControllerModel cm, Type type)
        {
            var attrList = new List<object>() { 0 };
            foreach (var method in type.GetMethods())
            {
                var actionModel = new ActionModel(method, attrList.AsReadOnly());
                actionModel.ActionName = method.Name;
                actionModel.Controller = cm;
                cm.Actions.Add(actionModel);
            }

        }
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            
        }

        public int Order => -850;
    }
}
