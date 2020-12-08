/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace OneScript.WebHost.Infrastructure
{
    public class OscriptViewsOverride : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var list = new List<string>();
            list.Add("/controllers/{1}/{0}.cshtml");
            list.AddRange(viewLocations.Select(x => x.ToLowerInvariant()));
            return list;
        }
    }
}
