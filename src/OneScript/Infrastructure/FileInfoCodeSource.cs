using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using ScriptEngine.Environment;

namespace OneScript.WebHost.Infrastructure
{
    public class FileInfoCodeSource : ICodeSource
    {
        private IFileInfo _fi;
        public FileInfoCodeSource(IFileInfo fi)
        {
            _fi = fi;
        }

        public string Code
        {
            get
            {
                using (var reader = new StreamReader(_fi.CreateReadStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public IFileInfo FileInfo => _fi;

        public string SourceDescription => _fi.PhysicalPath;
    }
}
