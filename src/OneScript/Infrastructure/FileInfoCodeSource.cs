/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
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
