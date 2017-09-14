using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OneScript.WebHost.Infrastructure
{
    public class OscriptApplicationModelProvider : IApplicationModelProvider
    {
        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            throw new NotImplementedException();
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            throw new NotImplementedException();
        }

        public int Order => -850;
    }
}
