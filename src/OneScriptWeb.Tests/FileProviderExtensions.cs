/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders;

namespace OneScriptWeb.Tests
{
    static class FileProviderExtensions
    {
        public static void AddFile(this InMemoryFileProvider fs, string pathAndName, string content)
        {
            var path = System.IO.Path.GetDirectoryName(pathAndName);
            if (path == "\\")
                path = "";
            
            var name = System.IO.Path.GetFileName(pathAndName);
            var strFi = new StringFileInfo(content, name);
            fs.Directory.AddFile(path, new FakeFileInfo(strFi, path));
        }
    }
}
