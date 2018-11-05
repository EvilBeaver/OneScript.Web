using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileSystemGlobbing;
using OneScript.WebHost.Application;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Reflection;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OscriptApplicationModelProvider : IApplicationModelProvider
    {
        private const string MODULE_FILENAME = "module.os";
        private readonly IApplicationRuntime _fw;
        private readonly IFileProvider _scriptsProvider;
        private readonly int _controllersMethodOffset;
        private readonly ApplicationInstance _app;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly ClassAttributeResolver _classAttribResolver;
        private readonly AnnotationAttributeMapper _annotationMapper = new AnnotationAttributeMapper();

        public OscriptApplicationModelProvider(ApplicationInstance appObject,
            IApplicationRuntime framework,
            IFileProvider sourceProvider,
            IAuthorizationPolicyProvider authPolicyProvider)
        {
            _fw = framework;
            _app = appObject;
            _scriptsProvider = sourceProvider;
            _controllersMethodOffset = ScriptedController.GetOwnMethodsRelectionOffset();
            _policyProvider = authPolicyProvider;
            _classAttribResolver = new ClassAttributeResolver();

            if (_fw.Engine.DirectiveResolver is DirectiveMultiResolver resolvers)
            {
                if (!resolvers.Any(x => x is ClassAttributeResolver))
                {
                    resolvers.Add(_classAttribResolver);
                }
            }

            FillDefaultMappers();

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
            var reflector = new TypeReflectionEngine();
            _fw.Environment.LoadMemory(MachineInstance.Current);
            foreach (var virtualPath in sources)
            {
                LoadedModule module;
                ICodeSource codeSrc;
                string typeName;
                if (virtualPath.IsDirectory)
                {
                    var info = FindModule(virtualPath.Name, MODULE_FILENAME) 
                               ?? FindModule(virtualPath.Name, virtualPath.Name+".os");

                    if(info == null)
                        continue;

                    codeSrc = new FileInfoCodeSource(info);
                    typeName = virtualPath.Name;

                }
                else
                {
                    codeSrc = new FileInfoCodeSource(virtualPath);
                    typeName = System.IO.Path.GetFileNameWithoutExtension(virtualPath.Name);
                }

                try
                {
                    _classAttribResolver.BeforeCompilation();
                    module = LoadControllerCode(codeSrc);
                }
                finally
                {
                    _classAttribResolver.AfterCompilation();
                }

                var reflectedType = reflector.Reflect<ScriptedController>(module, typeName);
                var attrList = MapAnnotationsToAttributes(_classAttribResolver.Attributes);
                var cm = new ControllerModel(typeof(ScriptedController).GetTypeInfo(), attrList.AsReadOnly());
                cm.ControllerName = reflectedType.Name;
                cm.Properties.Add("module", module);
                cm.Properties.Add("type", reflectedType);

                FillActions(cm, reflectedType);
                FillFilters(cm);

                context.Result.Controllers.Add(cm);
            }
        }

        private IFileInfo FindModule(string controllerName, string moduleName)
        {
            var path = Path.Combine("controllers", controllerName, moduleName);
            var info = _scriptsProvider.GetFileInfo(path);
            if (!info.Exists || info.IsDirectory)
                return null;

            return info;
        }

        private void FillFilters(ControllerModel cm)
        {
            foreach (var actionModel in cm.Actions)
            {
                var actionModelAuthData = actionModel.Attributes.OfType<IAuthorizeData>().ToArray();
                if (actionModelAuthData.Length > 0)
                {
                    actionModel.Filters.Add(GetFilter(_policyProvider, actionModelAuthData));
                }

                foreach (var attribute in actionModel.Attributes.OfType<IAllowAnonymous>())
                {
                    actionModel.Filters.Add(new AllowAnonymousFilter());
                }
            }
        }

        public static AuthorizeFilter GetFilter(IAuthorizationPolicyProvider policyProvider, IEnumerable<IAuthorizeData> authData)
        {
            // The default policy provider will make the same policy for given input, so make it only once.
            // This will always execute synchronously.
            if (policyProvider.GetType() == typeof(DefaultAuthorizationPolicyProvider))
            {
                var policy = AuthorizationPolicy.CombineAsync(policyProvider, authData).GetAwaiter().GetResult();
                return new AuthorizeFilter(policy);
            }
            else
            {
                return new AuthorizeFilter(policyProvider, authData);
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
            foreach (var method in type.GetMethods())
            {
                var scriptMethodInfo = method as ReflectedMethodInfo;
                if (scriptMethodInfo == null)
                    continue;

                var clrMethodInfo = MapToActionMethod(scriptMethodInfo);
                var attrList = MapAnnotationsToAttributes(scriptMethodInfo);
                var actionModel = new ActionModel(clrMethodInfo, attrList.AsReadOnly());
                actionModel.ActionName = method.Name;
                actionModel.Controller = cm;
                actionModel.Properties.Add("actionMethod", scriptMethodInfo);
                actionModel.Selectors.Add(new SelectorModel());
                cm.Actions.Add(actionModel);
            }

        }

        private List<object> MapAnnotationsToAttributes(ReflectedMethodInfo scriptMethodInfo)
        {
            var annotations = scriptMethodInfo.GetCustomAttributes(typeof(UserAnnotationAttribute), false)
                .Select(x=> ((UserAnnotationAttribute)x).Annotation);
         
            return MapAnnotationsToAttributes(annotations);
        }

        private List<object> MapAnnotationsToAttributes(IEnumerable<AnnotationDefinition> annotations)
        {
            var attrList = new List<object>();
            foreach (var annotation in annotations)
            {
                var attribute = _annotationMapper.Get(annotation);
                if(attribute != null)
                    attrList.Add(attribute);
            }

            return attrList;
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


        // TODO: Для быстрого старта. По мере развития будет заменяться на параметризованные версии
        private void MapDirect(string name, string alias, Type attrType)
        {
            _annotationMapper.AddMapper(name, alias, (anno) => Activator.CreateInstance(attrType));
        }

        private void FillDefaultMappers()
        {
            MapDirect("Authorize", "Авторизовать", typeof(AuthorizeAttribute));
            MapDirect("HttpPost", null, typeof(HttpPostAttribute));
            MapDirect("HttpGet", null, typeof(HttpGetAttribute));

            _annotationMapper.AddMapper("Http", MapHttpMethod);
            _annotationMapper.AddMapper("Action", "Действие", (annotation) =>
            {
                if (annotation.ParamCount != 1)
                    throw new AnnotationException(annotation, "Incorrect annotation parameter count");

                return new ActionNameAttribute(annotation.Parameters[0].RuntimeValue.AsString());
            });
        }

        private static object MapHttpMethod(AnnotationDefinition anno)
        {
            if (anno.ParamCount < 1)
                throw new AnnotationException(anno, "Missing parameter <Method>");

            var methodNames = anno.Parameters[0].RuntimeValue.AsString();
            if (anno.ParamCount == 2)
            {
                return new CustomHttpMethodAttribute(methodNames.Split(
                        new[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries),
                    anno.Parameters[1].RuntimeValue.AsString());
            }

            if (anno.ParamCount == 1)
            {
                return new CustomHttpMethodAttribute(methodNames.Split(
                    new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries));
            }

            throw new AnnotationException(anno, "Too many parameters");

        }
    }
}
