using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders;

namespace OneScriptWeb.Tests
{
    class FakeFileInfo : IFileInfo
    {
        private StringFileInfo _fi;
        
        public FakeFileInfo(StringFileInfo strFi, string path)
        {
            _fi = strFi;
            PhysicalPath = Path.Combine(path, strFi.Name);
        }
        
        public Stream CreateReadStream()
        {
            return _fi.CreateReadStream();
        }

        public bool Exists => _fi.Exists;

        public long Length => _fi.Length;

        public string PhysicalPath { get; }
    
        public string Name => _fi.Name;

        public DateTimeOffset LastModified => _fi.LastModified;

        public bool IsDirectory => _fi.IsDirectory;
    }
}
