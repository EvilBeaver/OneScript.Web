// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure
{
    public class DiEngineBuilder : DefaultEngineBuilder
    {
        public IServiceProvider Provider { get; set; }

        public DiEngineBuilder()
        {
        }

        protected override IServiceContainer GetContainer()
        {
            return new AspIoCImplementation(Provider);
        }
    }
}