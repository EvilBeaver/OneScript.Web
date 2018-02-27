using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class ScriptedViewComponentPartProvider : IApplicationFeatureProvider<ViewComponentFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewComponentFeature feature)
        {
            throw new NotImplementedException();
        }
    }
}
