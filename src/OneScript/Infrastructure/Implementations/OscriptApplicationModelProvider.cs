/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OneScript.WebHost.Application;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Reflection;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OscriptApplicationModelProvider : IApplicationModelProvider
    {
        private const string MODULE_FILENAME = "module.os";
        private const string CONTROLLERS_FOLDER = "controllers";
        
        private readonly IApplicationRuntime _fw;
        private readonly IFileProvider _scriptsProvider;
        private readonly ApplicationInstance _app;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly ClassAttributeResolver _classAttribResolver;
        private readonly AnnotationAttributeMapper _annotationMapper = new AnnotationAttributeMapper();
        private ILogger _logger;

        public OscriptApplicationModelProvider(ApplicationInstance appObject,
            IApplicationRuntime framework,
            IFileProvider sourceProvider,
            IAuthorizationPolicyProvider authPolicyProvider)
        {
            _fw = framework;
            _app = appObject;
            _scriptsProvider = sourceProvider;
            _policyProvider = authPolicyProvider;
            _classAttribResolver = new ClassAttributeResolver();

            if (!_fw.Engine.DirectiveResolvers.Any(x => x is ClassAttributeResolver))
            {
                _fw.Engine.DirectiveResolvers.Add(_classAttribResolver);
            } 
            
            FillDefaultMappers();
        }
        
        public OscriptApplicationModelProvider(ApplicationInstance appObject,
            IApplicationRuntime framework,
            IFileProvider sourceProvider,
            IAuthorizationPolicyProvider authPolicyProvider,
            ILoggerFactory loggerFactory):this(appObject,framework,sourceProvider,authPolicyProvider)
        {
            _logger = loggerFactory.CreateLogger(GetType());
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
                var filesystemSources = _scriptsProvider.GetDirectoryContents(CONTROLLERS_FOLDER).Where(x => !x.IsDirectory && x.PhysicalPath.EndsWith(".os"));
                sources.AddRange(filesystemSources);

                // контроллеры в папках
                filesystemSources = _scriptsProvider.GetDirectoryContents(CONTROLLERS_FOLDER)
                    .Where(x => x.IsDirectory);

                sources.AddRange(filesystemSources);
            }

            FillContext(sources, context);
        }

        private void FillContext(IEnumerable<IFileInfo> sources, ApplicationModelProviderContext context)
        {
            var reflector = new TypeReflectionEngine();
            MachineInstance.Current.PrepareThread(_fw.Environment);
            foreach (var virtualPath in sources)
            {
                LoadedModule module;
                FileInfoCodeSource codeSrc;
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

                module = CompileControllerModule(codeSrc);

                var reflectedType = reflector.Reflect<ScriptedController>(module, typeName);
                var attrList = MapAnnotationsToAttributes(_classAttribResolver.Attributes);
                var cm = new ControllerModel(typeof(ScriptedController).GetTypeInfo(), attrList.AsReadOnly());
                cm.ControllerName = reflectedType.Name;
                var recompileInfo = new DynamicCompilationInfo()
                {
                    Module = module,
                    CodeSource = codeSrc,
                    Tag = cm
                };
                
                cm.Properties.Add("CompilationInfo", recompileInfo);
                cm.Properties.Add("type", reflectedType);
                
                ChangeToken.OnChange(()=>CreateWatchToken(codeSrc),
                    RecompileController,
                    recompileInfo);
                
                FillActions(cm, reflectedType);
                FillFilters(cm);

                context.Result.Controllers.Add(cm);
            }
        }

        private IChangeToken CreateWatchToken(FileInfoCodeSource codeSrc)
        {
            var cPath = codeSrc.FileInfo.PhysicalPath.Replace('\\', '/');
            var root = Path.GetDirectoryName(_app.Module.ModuleInfo.Origin).Replace('\\','/');
            if (!cPath.StartsWith(root))
            {
                _logger.LogWarning($"file {cPath} can't be watched since it's not in root folder {root}");
                return NullChangeToken.Singleton;
            }
            
            return _scriptsProvider.Watch(cPath.Substring(root.Length));
        }

        private LoadedModule CompileControllerModule(ICodeSource codeSrc)
        {
            LoadedModule module;
            try
            {
                _fw.DebugCurrentThread();
                _classAttribResolver.BeforeCompilation();
                module = LoadControllerCode(codeSrc);
            }
            finally
            {
                _fw.StopDebugCurrentThread();
                _classAttribResolver.AfterCompilation();
            }

            return module;
        }

        private void RecompileController(DynamicCompilationInfo info)
        {
            if (info.TimeStamp != default(DateTime))
            {
                if ((DateTime.Now - info.TimeStamp).Seconds < 1)
                {
                    _logger.LogDebug("Skipping duplicate change token");
                    return;
                }
            }

            var controllerModel = (ControllerModel) info.Tag;
            _logger?.LogInformation($"Start recompiling controller {controllerModel.ControllerName}");

            var module = CompileControllerModule(info.CodeSource);
            var canRecompile = controllerModel.Attributes.OrderBy(x => x.GetType())
                .SequenceEqual(MapAnnotationsToAttributes(_classAttribResolver.Attributes)
                    .OrderBy(x => x.GetType()));
            
            if (!canRecompile)
            {
                _logger?.LogError($"Can't recompile controller {controllerModel.ControllerName} when list of attributes has changed");
                return;
            }

            info.Module = module;
            info.TimeStamp = DateTime.Now;
            _logger?.LogInformation($"Controller {controllerModel.ControllerName} has been recompiled");

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
            var compiler = _fw.GetCompilerService();
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

                var actionModel = CreateActionModel(scriptMethodInfo);
                actionModel.Controller = cm;
                cm.Actions.Add(actionModel);
            }

        }

        private ActionModel CreateActionModel(ReflectedMethodInfo scriptMethodInfo)
        {
            var clrMethodInfo = MapToActionMethod(scriptMethodInfo);
            var userAnnotations = scriptMethodInfo.GetCustomAttributes(typeof(UserAnnotationAttribute), false)
                .Select(x => ((UserAnnotationAttribute) x).Annotation);

            string actionName = scriptMethodInfo.Name;
            string magicHttpMethod = null;
            List<AnnotationDefinition> workSet = new List<AnnotationDefinition>();

            var pos = actionName.LastIndexOf('_');
            if (pos > 0 && pos < actionName.Length - 1)
            {
                magicHttpMethod = actionName.Substring(pos + 1);
            }

            foreach (var annotation in userAnnotations)
            {
                var loCase = annotation.Name.ToLowerInvariant();
                var workAnnotation = annotation;
                if (loCase == "httpmethod")
                {
                    if (magicHttpMethod != null)
                    {
                        if (annotation.ParamCount == 0)
                        {
                            // наш случай.
                            actionName = actionName.Substring(0, pos);
                            workAnnotation.Parameters = new[]
                            {
                                new AnnotationParameter()
                                {
                                    Name = "Method",
                                    RuntimeValue = ValueFactory.Create(magicHttpMethod)
                                }
                            };
                        }
                    }
                }
                
                workSet.Add(workAnnotation);

            }

            var attrList = MapAnnotationsToAttributes(workSet);
            var annotatedActionName = attrList.OfType<ActionNameAttribute>().FirstOrDefault();
            if (annotatedActionName?.Name != null)
            {
                actionName = annotatedActionName.Name;
            }
            
            var actionModel = new ActionModel(clrMethodInfo, attrList.AsReadOnly());
            actionModel.ActionName = actionName;
            actionModel.Properties.Add("actionMethod", scriptMethodInfo);
            foreach(var selector in CreateSelectors(actionModel.Attributes))
                actionModel.Selectors.Add(selector);

            return actionModel;
        }

        private IEnumerable<SelectorModel> CreateSelectors(IEnumerable<object> attributes)
        {
            var routeProviders = new List<IRouteTemplateProvider>();
            var createSelectorForSilentRouteProviders = false;
            foreach (var attribute in attributes)
            {
                if (attribute is IRouteTemplateProvider routeTemplateProvider)
                {
                    if (IsSilentRouteAttribute(routeTemplateProvider))
                    {
                        createSelectorForSilentRouteProviders = true;
                    }
                    else
                    {
                        routeProviders.Add(routeTemplateProvider);
                    }
                }
            }

            foreach (var routeProvider in routeProviders)
            {
                // If we see an attribute like
                // [Route(...)]
                //
                // Then we want to group any attributes like [HttpGet] with it.
                //
                // Basically...
                //
                // [HttpGet]
                // [HttpPost("Products")]
                // public void Foo() { }
                //
                // Is two selectors. And...
                //
                // [HttpGet]
                // [Route("Products")]
                // public void Foo() { }
                //
                // Is one selector.
                if (!(routeProvider is IActionHttpMethodProvider))
                {
                    createSelectorForSilentRouteProviders = false;
                }
            }

            var selectorModels = new List<SelectorModel>();
            if (routeProviders.Count == 0 && !createSelectorForSilentRouteProviders)
            {
                // Simple case, all attributes apply
                selectorModels.Add(CreateSelectorModel(route: null, attributes: attributes));
            }
            else
            {
                // Each of these routeProviders are the ones that actually have routing information on them
                // something like [HttpGet] won't show up here, but [HttpGet("Products")] will.
                foreach (var routeProvider in routeProviders)
                {
                    var filteredAttributes = new List<object>();
                    foreach (var attribute in attributes)
                    {
                        if (ReferenceEquals(attribute, routeProvider))
                        {
                            filteredAttributes.Add(attribute);
                        }
                        else if (InRouteProviders(routeProviders, attribute))
                        {
                            // Exclude other route template providers
                            // Example:
                            // [HttpGet("template")]
                            // [Route("template/{id}")]
                        }
                        else if (
                            routeProvider is IActionHttpMethodProvider &&
                            attribute is IActionHttpMethodProvider)
                        {
                            // Example:
                            // [HttpGet("template")]
                            // [AcceptVerbs("GET", "POST")]
                            //
                            // Exclude other http method providers if this route is an
                            // http method provider.
                        }
                        else
                        {
                            filteredAttributes.Add(attribute);
                        }
                    }

                    selectorModels.Add(CreateSelectorModel(routeProvider, filteredAttributes));
                }

                if (createSelectorForSilentRouteProviders)
                {
                    var filteredAttributes = new List<object>();
                    foreach (var attribute in attributes)
                    {
                        if (!InRouteProviders(routeProviders, attribute))
                        {
                            filteredAttributes.Add(attribute);
                        }
                    }

                    selectorModels.Add(CreateSelectorModel(route: null, attributes: filteredAttributes));
                }
            }

            return selectorModels;
        }

        private bool IsSilentRouteAttribute(IRouteTemplateProvider routeTemplateProvider)
        {
            return
                routeTemplateProvider.Template == null &&
                routeTemplateProvider.Order == null &&
                routeTemplateProvider.Name == null;
        }

        private static bool InRouteProviders(List<IRouteTemplateProvider> routeProviders, object attribute)
        {
            foreach (var rp in routeProviders)
            {
                if (ReferenceEquals(rp, attribute))
                {
                    return true;
                }
            }

            return false;
        }

        private static SelectorModel CreateSelectorModel(IRouteTemplateProvider route, IEnumerable<object> attributes)
        {
            var selectorModel = new SelectorModel();
            if (route != null)
            {
                selectorModel.AttributeRouteModel = new AttributeRouteModel(route);
            }

            foreach (var constraint in attributes.OfType<IActionConstraintMetadata>())
            {
                selectorModel.ActionConstraints.Add(constraint);
            }
            
            // Simple case, all HTTP method attributes apply
            var httpMethods = attributes
                .OfType<IActionHttpMethodProvider>()
                .SelectMany(a => a.HttpMethods)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (httpMethods.Length > 0)
            {
                selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(httpMethods));
            }

            return selectorModel;
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
            MapDirect("HttpPost", null, typeof(HttpPostAttribute));
            MapDirect("HttpGet", null, typeof(HttpGetAttribute));

            _annotationMapper.AddMapper("HttpMethod", MapHttpMethod);
            _annotationMapper.AddMapper("Authorize", "Авторизовать", MapAuthorizationAttribute);
            _annotationMapper.AddMapper("Action", "Действие", (annotation) =>
            {
                if (annotation.ParamCount != 1)
                    throw new AnnotationException(annotation, "Incorrect annotation parameter count");

                return new ActionNameAttribute(annotation.Parameters[0].RuntimeValue.AsString());
            });

            // TODO: refactor me
            _annotationMapper.AddMapper("Route", "Маршрут", (annotation) =>
            {
                if (annotation.ParamCount != 1)
                    throw new AnnotationException(annotation, "Incorrect annotation parameter count");

                return new RouteAttribute(annotation.Parameters[0].RuntimeValue.AsString());
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

        private static object MapAuthorizationAttribute(AnnotationDefinition anno)
        {
            var instance = new AuthorizeAttribute();
            if (anno.ParamCount == 0)
                return instance;
            
            foreach (var parameter in anno.Parameters)
            {
                if (BiLingualEquals(parameter.Name, "roles", "роли"))
                {
                    instance.Roles = parameter.RuntimeValue.AsString();
                }
                else if (BiLingualEquals(parameter.Name, "policy", "политика"))
                {
                    instance.Policy = parameter.RuntimeValue.AsString();
                }
            }

            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool BiLingualEquals(string src, string en, string ru)
        {
            return src.Equals(en, StringComparison.OrdinalIgnoreCase) ||
                   src.Equals(ru, StringComparison.OrdinalIgnoreCase);
        }
    }
}
