/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace OneScript.WebHost.Authorization
{
    public static class CustomAuthExtensions
    {
        public static void AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandlerProvider, OneScriptAuthorizationHandlerProvider>();
        }
    }
}
