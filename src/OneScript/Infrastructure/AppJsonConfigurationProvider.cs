// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using ScriptEngine.HostedScript;

namespace OneScript.WebHost.Infrastructure
{
    public class AppJsonConfigurationProvider : IConfigProvider
    {
        private readonly IConfiguration _config;

        public AppJsonConfigurationProvider(IConfiguration config)
        {
            _config = config;
        }
        
        public Func<IDictionary<string, string>> GetProvider()
        {
            return () => _config.GetSection("OneScript")
                    .GetChildren()
                    .ToDictionary(x => x.Key, ExtractValue);
        }

        private static string ExtractValue(IConfigurationSection configurationSection)
        {
            if (configurationSection.Value != default)
                return configurationSection.Value;

            var enumerable = configurationSection
                .GetChildren()
                .AsEnumerable()
                .Select(x => x.Value);
            return string.Join(',', enumerable);
        }

        public static Func<IDictionary<string, string>> FromConfigurationOptions(IConfiguration config)
        {
            return new AppJsonConfigurationProvider(config).GetProvider();
        }
    }
}