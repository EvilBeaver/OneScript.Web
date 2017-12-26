using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization.Configuration;
using Microsoft.AspNetCore.Mvc.Razor;

namespace OneScript.WebHost.Infrastructure
{
    public class OscriptViewsOverride : IViewLocationExpander
    {
        private string _rootPath;

        public OscriptViewsOverride(string rootPath)
        {
            _rootPath = rootPath;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {    
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var enumer = viewLocations.Select(x => Path.Combine(_rootPath, x.Substring(1)));
            foreach (var VARIABLE in enumer)
            {
                Console.WriteLine(VARIABLE);
            }

            return enumer;
        }
    }
}
