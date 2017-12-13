using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OscriptApplicationModelProvider : IApplicationModelProvider
    {
        private readonly IApplicationRuntime _fw;
        private readonly IScriptsProvider _scriptsProvider;
        public OscriptApplicationModelProvider(IApplicationRuntime framework, IScriptsProvider sourceProvider)
        {
            _fw = framework;
            _scriptsProvider = sourceProvider;
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            var attrList = new List<string>();
            var sources = _scriptsProvider.EnumerateFiles("/controllers");
            var reflector = new TypeReflectionEngine();
            foreach (var virtualPath in sources)
            {
                var codeSrc = _scriptsProvider.Get(virtualPath);
                var module = LoadControllerCode(codeSrc);
                var baseFileName = System.IO.Path.GetFileNameWithoutExtension(codeSrc.SourceDescription);
                var type = reflector.Reflect(module, baseFileName);
                var cm = new ControllerModel(typeof(ScriptedController).GetTypeInfo(), attrList.AsReadOnly());
                cm.ControllerName = type.Name;
                cm.Properties.Add("module", module);
                cm.Properties.Add("type", type);
                FillActions(cm, type);

                context.Result.Controllers.Add(cm);
            }
        }

        private LoadedModuleHandle LoadControllerCode(ICodeSource src)
        {
            var compiler = _fw.Engine.GetCompilerService();
            // TODO: inject compiler own symbols
            var byteCode = compiler.CreateModule(src);
            return _fw.Engine.LoadModuleImage(byteCode);
        }

        private void FillActions(ControllerModel cm, Type type)
        {
            var attrList = new List<object>() { 0 };
            foreach (var method in type.GetMethods())
            {
                var scriptMethodInfo = method as ReflectedMethodInfo;
                var clrMethodInfo = typeof(ScriptedController).GetMethod("VoidAction");
                var actionModel = new ActionModel(clrMethodInfo, attrList.AsReadOnly());
                actionModel.ActionName = method.Name;
                actionModel.Controller = cm;
                actionModel.Properties.Add("actionMethod", scriptMethodInfo);
                actionModel.Selectors.Add(new SelectorModel());
                cm.Actions.Add(actionModel);
            }

        }
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            
        }

        public int Order => -850;
    }
}
