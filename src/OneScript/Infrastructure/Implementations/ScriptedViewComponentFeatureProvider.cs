using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using OneScript.WebHost.Application;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Reflection;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class ScriptedViewComponentFeatureProvider : IApplicationFeatureProvider<ViewComponentFeature>
    {
        public ApplicationInstance Application { get; set;  }
        public IScriptsProvider ScriptsProvider { get; set; }
        public IApplicationRuntime Framework { get; set; }

        private TypeInfo[] _discoveredTypes;

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

            Application.OnViewComponentsCreation(out files, ref standardHandling);

            var sources = new List<string>();
            if (files != null)
                sources.AddRange(files);

            if (standardHandling)
            {
                var filesystemSources = ScriptsProvider.EnumerateFiles("/viewComponents");
                sources.AddRange(filesystemSources);
            }

            FillFeature(sources);
        }

        private void FillFeature(List<string> sources)
        {
            var typeInfos = new List<TypeInfo>();
            foreach (var virtualPath in sources)
            {
                var code = ScriptsProvider.Get(virtualPath);
                var compiler = Framework.Engine.GetCompilerService();
                var img = compiler.Compile(code);
                var module = Framework.Engine.LoadModuleImage(img);
                var baseFileName = System.IO.Path.GetFileNameWithoutExtension(code.SourceDescription);

                var builder = new ClassBuilder<ScriptedViewComponent>();
                var type = builder.SetModule(module)
                    .SetTypeName(baseFileName + "ViewComponent")
                    .ExportMethods()
                    .ExportProperties()
                    .ExportConstructor((parameters) => new ScriptedViewComponent(builder.Module, builder.TypeName))
                    // TODO: Раскомментировать после выпуска ошибки рефлексии в preview10
                    //.ExportClassMethod("Invoke") 
                    .Build();

                // обход ошибки с недобавлением нативного метода в ClassBuilder
                var refl = type as ReflectedClassType<ScriptedViewComponent>;
                Debug.Assert(refl != null);

                var allMethods = refl.GetMethods().ToList();
                allMethods.Add(typeof(ScriptedViewComponent).GetMethod("Invoke"));
                refl.SetMethods(allMethods);
                // конец костыля

                typeInfos.Add(type.GetTypeInfo());
            }

            _discoveredTypes = typeInfos.ToArray();
        }
    }
}
