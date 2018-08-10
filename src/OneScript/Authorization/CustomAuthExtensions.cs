using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace OneScript.WebHost.Authorization
{
    public static class CustomAuthExtensions
    {
        public const string ConfigSectionName = "Authorization:Custom";

        public static void AddCustomAuthorization(this IServiceCollection services, IFileProvider filesystem)
        {
            var authFile = filesystem.GetFileInfo("auth.os");
            if (authFile.Exists && !authFile.IsDirectory)
            {
            }
        }
    }
}
