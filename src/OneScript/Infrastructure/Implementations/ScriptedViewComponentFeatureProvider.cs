using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using OneScript.WebHost.Application;
using ScriptEngine.Machine.Reflection;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class ScriptedViewComponentFeatureProvider : IApplicationFeatureProvider<ViewComponentFeature>
    {
        private readonly ApplicationInstance _app;
        private readonly IScriptsProvider _scriptsProvider;
        private readonly IApplicationRuntime _fw;

        private TypeInfo[] _discoveredTypes;

        public ScriptedViewComponentFeatureProvider(ApplicationPartManager partManager, IApplicationRuntime framework, ApplicationInstance app, IScriptsProvider fsProvider)
        {
            _app = app;
            _fw = framework;
            _scriptsProvider = fsProvider;
            partManager.FeatureProviders.Add(this);
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewComponentFeature feature)
        {
            if(_discoveredTypes == null)
                DiscoverViewComponents();

            foreach (var type in _discoveredTypes)
            {
                feature.ViewComponents.Add(type);
            }
        }

        private void DiscoverViewComponents()
        {
            IEnumerable<string> files;
            bool standardHandling = true;

            _app.OnViewComponentsCreation(out files, ref standardHandling);

            var sources = new List<string>();
            if (files != null)
                sources.AddRange(files);

            if (standardHandling)
            {
                var filesystemSources = _scriptsProvider.EnumerateFiles("/viewComponents");
                sources.AddRange(filesystemSources);
            }

            FillFeature(sources);
        }

        private void FillFeature(List<string> sources)
        {
            var typeInfos = new List<TypeInfo>();
            foreach (var virtualPath in sources)
            {
                var code = _scriptsProvider.Get(virtualPath);
                var compiler = _fw.Engine.GetCompilerService();
                var img = compiler.Compile(code);
                var module = _fw.Engine.LoadModuleImage(img);
                var baseFileName = System.IO.Path.GetFileNameWithoutExtension(code.SourceDescription);

                var builder = new ClassBuilder<ScriptedViewComponent>();
                var type = builder.SetModule(module)
                    .SetTypeName(baseFileName + "ViewComponent")
                    .ExportMethods()
                    .ExportProperties()
                    .ExportConstructor((parameters) => new ScriptedViewComponent(builder.Module, builder.TypeName))
                    .Build();

                typeInfos.Add(type.GetTypeInfo());
            }

            _discoveredTypes = typeInfos.ToArray();
        }
    }
}
