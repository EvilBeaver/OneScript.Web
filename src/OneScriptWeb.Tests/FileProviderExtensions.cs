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
