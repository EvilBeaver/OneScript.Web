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
                var type = reflector.Reflect(module, baseFileName);
                var cm = new ControllerModel(typeof(ScriptedController).GetTypeInfo(), attrList.AsReadOnly());
                cm.ControllerName = type.Name;
                cm.Properties.Add("module", module);
                cm.Properties.Add("type", type);
                FillActions(cm, type, GetExportedMethods(module));

                context.Result.Controllers.Add(cm);
            }
        }

        // TODO: костыль для борьбы с https://github.com/EvilBeaver/OneScript/issues/626
        private IEnumerable<string> GetExportedMethods(LoadedModuleHandle module)
        {
            var type = typeof(LoadedModuleHandle);
            var reflectedModule = type.InvokeMember("Module", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty, null, module, new object[]{});

            var modType = reflectedModule.GetType();
            var propMethods = modType.GetProperty("ExportedMethods");
            var exports = propMethods.GetValue(reflectedModule) as Array;
            Debug.Assert(exports != null);

            var names = new List<string>();

            foreach (var obj in exports)
            {
                var exportType = obj.GetType();
                var propName = exportType.GetField("SymbolicName");
                names.Add((string)propName.GetValue(obj));
            }

            return names;
        }

        private LoadedModuleHandle LoadControllerCode(ICodeSource src)
        {
            var compiler = _fw.Engine.GetCompilerService();
            var byteCode = ScriptedController.CompileModule(compiler, src);
            return _fw.Engine.LoadModuleImage(byteCode);
        }

        private void FillActions(ControllerModel cm, Type type, IEnumerable<string> exports)
        {
            var attrList = new List<object>() { 0 };
            foreach (var method in type.GetMethods().Where(x=>exports.Contains(x.Name)))
            {
                var scriptMethodInfo = method as ReflectedMethodInfo;
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

        private static MethodInfo MapToActionMethod(ReflectedMethodInfo reflectedMethodInfo)
        {
            MethodInfo clrMethodInfo;
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
