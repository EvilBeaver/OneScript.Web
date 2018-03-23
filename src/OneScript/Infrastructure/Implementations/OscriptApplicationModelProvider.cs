using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OneScript.WebHost.Application;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OscriptApplicationModelProvider : IApplicationModelProvider
    {
        private readonly IApplicationRuntime _fw;
        private readonly IScriptsProvider _scriptsProvider;
        private readonly int _controllersMethodOffset;
        private readonly ApplicationInstance _app;

        public OscriptApplicationModelProvider(ApplicationInstance appObject, IApplicationRuntime framework, IScriptsProvider sourceProvider)
        {
            _fw = framework;
            _app = appObject;
            _scriptsProvider = sourceProvider;
            _controllersMethodOffset = ScriptedController.GetOwnMethodsRelectionOffset();
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            IEnumerable<string> files;
            bool standardHandling = true;

            _app.OnControllersCreation(out files, ref standardHandling);

            var sources = new List<string>();
            if (files != null)
                sources.AddRange(files);

            if (standardHandling)
            {
                var filesystemSources = _scriptsProvider.EnumerateFiles("/controllers");
                sources.AddRange(filesystemSources);
            }

            FillContext(sources, context);
        }

        private void FillContext(IEnumerable<string> sources, ApplicationModelProviderContext context)
        {
            var attrList = new List<string>();
            var reflector = new TypeReflectionEngine();
            foreach (var virtualPath in sources)
            {
                var codeSrc = _scriptsProvider.Get(virtualPath);
                var module = LoadControllerCode(codeSrc);
                var baseFileName = System.IO.Path.GetFileNameWithoutExtension(codeSrc.SourceDescription);
                var type = reflector.Reflect<ScriptedController>(module, baseFileName);
                var cm = new ControllerModel(typeof(ScriptedController).GetTypeInfo(), attrList.AsReadOnly());
                cm.ControllerName = type.Name;
                cm.Properties.Add("module", module);
                cm.Properties.Add("type", type);
                FillActions(cm, type);

                context.Result.Controllers.Add(cm);
            }
        }
        
        private LoadedModule LoadControllerCode(ICodeSource src)
        {
            var compiler = _fw.Engine.GetCompilerService();
            var byteCode = ScriptedController.CompileModule(compiler, src);
            return _fw.Engine.LoadModuleImage(byteCode);
        }

        private void FillActions(ControllerModel cm, Type type)
        {
            var attrList = new List<object>() { 0 };
            foreach (var method in type.GetMethods())
            {
                var scriptMethodInfo = method as ReflectedMethodInfo;
                if (scriptMethodInfo == null)
                    continue;

                CorrectDispId(scriptMethodInfo);
                var clrMethodInfo = MapToActionMethod(scriptMethodInfo);
                var actionModel = new ActionModel(clrMethodInfo, attrList.AsReadOnly());
                actionModel.ActionName = method.Name;
                actionModel.Controller = cm;
                actionModel.Properties.Add("actionMethod", scriptMethodInfo);
                actionModel.Selectors.Add(new SelectorModel());
                cm.Actions.Add(actionModel);
            }

        }

        private void CorrectDispId(ReflectedMethodInfo scriptMethodInfo)
        {
            var fieldId = typeof(ReflectedMethodInfo).GetField("_dispId",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

            var currId = (int)fieldId.GetValue(scriptMethodInfo);
            currId += _controllersMethodOffset;
            fieldId.SetValue(scriptMethodInfo, currId);
        }

        private static System.Reflection.MethodInfo MapToActionMethod(ReflectedMethodInfo reflectedMethodInfo)
        {
            System.Reflection.MethodInfo clrMethodInfo;
            if (reflectedMethodInfo.IsFunction)
            {
                clrMethodInfo = typeof(ScriptedController).GetMethod("ResultAction");
            }
            else
            {
                clrMethodInfo = typeof(ScriptedController).GetMethod("VoidAction");
            }
            return clrMethodInfo;
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            
        }

        public int Order => -850;
    }
}
