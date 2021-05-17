// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System;
using ScriptEngine.Hosting;

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