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
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using OneScript.WebHost.Application;
using ScriptEngine.Machine.Reflection;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class ScriptedViewComponentFeatureProvider : IApplicationFeatureProvider<ViewComponentFeature>
    {
        public ApplicationInstance Application { get; set;  }
        public IFileProvider ScriptsProvider { get; set; }
        public IApplicationRuntime Runtime { get; set; }

        private TypeInfo[] _discoveredTypes;

        public void Configure(IServiceProvider services)
        {
            // в режиме тестирования app-instance может быть null
            Application = services.GetService<ApplicationInstance>();
            ScriptsProvider = services.GetRequiredService<IFileProvider>();
            Runtime = services.GetRequiredService<IApplicationRuntime>();
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
            IEnumerable<string> files = null;
            bool standardHandling = true;

            Application?.OnViewComponentsCreation(out files, ref standardHandling);

            var sources = new List<IFileInfo>();
            if (files != null)
                sources.AddRange(files.Select(x=>new PhysicalFileInfo(new FileInfo(x))));

            if (standardHandling)
            {
                var filesystemSources = ScriptsProvider.GetDirectoryContents("/viewComponents");
                sources.AddRange(filesystemSources);
            }

            FillFeature(sources);
        }

        private void FillFeature(List<IFileInfo> sources)
        {
            var typeInfos = new List<TypeInfo>();
            foreach (var virtualPath in sources)
            {
                var code = new FileInfoCodeSource(virtualPath);
                var compiler = Runtime.GetCompilerService();
                var img = ScriptedViewComponent.CompileModule(compiler,code);
                var invokatorExist = img.Methods.Any(x =>
                    StringComparer.OrdinalIgnoreCase.Compare(ScriptedViewComponent.InvokeMethodNameRu, x.Signature.Name) == 0
                    || StringComparer.OrdinalIgnoreCase.Compare(ScriptedViewComponent.InvokeMethodNameEn, x.Signature.Name) == 0);

                if(!invokatorExist)
                    continue;

                var module = Runtime.Engine.LoadModuleImage(img);
                var baseFileName = Path.GetFileNameWithoutExtension(code.SourceDescription);

                var builder = new ClassBuilder<ScriptedViewComponent>();
                var type = builder.SetModule(module)
                    .SetTypeName(baseFileName + "ViewComponent")
                    .ExportMethods()
                    .ExportProperties()
                    .ExportConstructor((parameters) => new ScriptedViewComponent(builder.Module, builder.TypeName))
                    .ExportClassMethod("Invoke") 
                    .Build();

                typeInfos.Add(type.GetTypeInfo());
            }

            _discoveredTypes = typeInfos.ToArray();
        }
    }
}
