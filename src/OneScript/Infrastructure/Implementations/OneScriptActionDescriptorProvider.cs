using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class OneScriptActionDescriptorProvider : IActionDescriptorProvider
    {
        public void OnProvidersExecuting(ActionDescriptorProviderContext context)
        {
            
        }

        public void OnProvidersExecuted(ActionDescriptorProviderContext context)
        {
            
        }

        public int Order => -950;
    }
}
