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
        private readonly OneScriptScriptFactory _fw;
        public OscriptApplicationModelProvider(OneScriptScriptFactory framework)
        {
            _fw = framework;
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            var type = typeof(ScriptedController).GetTypeInfo();
            var attrList = new List<string>();
            var sources = _fw.SourceProvider.EnumerateEntries("/controllers");
            foreach (var item in sources)
            {
                var cm = new ControllerModel(type, null);
                
            }

            //var list = new List<string>();
            //list.Add("wtf are attributes?");

            //var cm = new ControllerModel(typeof(ScriptedController).GetTypeInfo(), list.AsReadOnly());
            //cm.ControllerName = "c1";
            //cm.Properties["script"] = "dummysource.os";

            //var cm1 = new ControllerModel(typeof(ScriptedController).GetTypeInfo(), list.AsReadOnly());
            //cm1.Properties["script"] = "dummysource2.os";
            //cm1.ControllerName = "c2";
           
            //context.Result.Controllers.Add(cm);
            //context.Result.Controllers.Add(cm1);

        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            
        }

        public int Order => -850;
    }
}
