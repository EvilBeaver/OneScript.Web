using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace OneScript.WebHost.Infrastructure
{
    public static class MvcMiddlewareExtensions
    {
        public static void UseOscriptMvc(this IApplicationBuilder app)
        {
            app.Use((context, next) =>
            {
                var rd = context.GetRouteData();
                return next();
            });
        }
    }

}
