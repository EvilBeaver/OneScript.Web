/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
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
