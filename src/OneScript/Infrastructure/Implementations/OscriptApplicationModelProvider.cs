using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileSystemGlobbing;
using OneScript.WebHost.Application;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OscriptApplicationModelProvider : IApplicationModelProvider
    {
        private const string MODULE_FILENAME = "module.os";
        private readonly IApplicationRuntime _fw;
        private readonly IFileProvider _scriptsProvider;
        private readonly int _controllersMethodOffset;
        private readonly ApplicationInstance _app;

        public OscriptApplicationModelProvider(ApplicationInstance appObject, IApplicationRuntime framework, IFileProvider sourceProvider)
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

            var sources = new List<IFileInfo>();
            if (files != null)
                sources.AddRange(files.Select(x => new PhysicalFileInfo(new FileInfo(x))));

            if (standardHandling)
            {
                // прямые контроллеры
                var filesystemSources = _scriptsProvider.GetDirectoryContents("controllers").Where(x => !x.IsDirectory && x.PhysicalPath.EndsWith(".os"));
                sources.AddRange(filesystemSources);

                // контроллеры в папках
                filesystemSources = _scriptsProvider.GetDirectoryContents("controllers")
                    .Where(x => x.IsDirectory);

                sources.AddRange(filesystemSources);
            }

            FillContext(sources, context);
        }

        private void FillContext(IEnumerable<IFileInfo> sources, ApplicationModelProviderContext context)
        {
            var attrList = new List<string>();
            var reflector = new TypeReflectionEngine();
            _fw.Environment.LoadMemory(MachineInstance.Current);
            foreach (var virtualPath in sources)
            {
                Type reflectedType;
                LoadedModule module;
                if (virtualPath.IsDirectory)
                {
                    var path = Path.Combine("controllers", virtualPath.Name, MODULE_FILENAME);
                    var info = _scriptsProvider.GetFileInfo(path);
                    if(!info.Exists || info.IsDirectory)
                        continue;

                    var codeSrc = new FileInfoCodeSource(info);
                    module = LoadControllerCode(codeSrc);
                    reflectedType = reflector.Reflect<ScriptedController>(module, virtualPath.Name);
                }
                else
                {
                    var codeSrc = new FileInfoCodeSource(virtualPath);
                    module = LoadControllerCode(codeSrc);
                    var baseFileName = System.IO.Path.GetFileNameWithoutExtension(virtualPath.Name);
                    reflectedType = reflector.Reflect<ScriptedController>(module, baseFileName);
                    
                }

                var cm = new ControllerModel(typeof(ScriptedController).GetTypeInfo(), attrList.AsReadOnly());
                cm.ControllerName = reflectedType.Name;
                cm.Properties.Add("module", module);
                cm.Properties.Add("type", reflectedType);

                FillActions(cm, reflectedType);

                context.Result.Controllers.Add(cm);
            }
        }
        


        private LoadedModule LoadControllerCode(ICodeSource src)
        {
            var compiler = _fw.Engine.GetCompilerService();
            compiler.DefineVariable("ЭтотОбъект", "ThisObject", SymbolType.ContextProperty);
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
