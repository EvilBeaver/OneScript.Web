/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace OneScriptWeb.Tests
{

    // Класс-костыль для обхода проблемы
    // https://github.com/EvilBeaver/OneScript/issues/623
    static class TestOrderingLock
    {

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static TestOrderingLock() { }
        
        static object _lock = new object();

        public static object Lock
        {
            get { return _lock; }
        }
    }
}
