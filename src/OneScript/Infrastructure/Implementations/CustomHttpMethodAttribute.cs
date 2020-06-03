using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Routing;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class CustomHttpMethodAttribute : HttpMethodAttribute
    {
        public CustomHttpMethodAttribute(IEnumerable<string> httpMethods) : base(httpMethods)
        {
        }

        public CustomHttpMethodAttribute(IEnumerable<string> httpMethods, string template) : base(httpMethods, template)
        {
        }
    }
}
