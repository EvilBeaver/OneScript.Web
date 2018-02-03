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
