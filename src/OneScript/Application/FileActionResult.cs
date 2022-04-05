/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("РезультатДействияФайл")]
    public class FileActionResult : AutoContext<FileActionResult>, IActionResult, IObjectWrapper
    {
        private object src;

        public FileActionResult(string file, string contentType)
        {
            src = new FileStream(file, FileMode.Open, FileAccess.Read);
            ContentType = contentType ?? "application/octet-stream";
            LastModified = File.GetLastWriteTimeUtc(file);
        }

        public FileActionResult(BinaryDataContext binaryData, string contentType)
        {
            src = binaryData;
            ContentType = contentType;
        }

        [ContextProperty("ТипСодержимого")]
        public string ContentType { get; set; }

        [ContextProperty("ИмяПолучаемогоФайла")]
        public string DownloadFileName { get; set; }

        [ContextProperty("ВремяИзменения")]
        public DateTime LastModified { get; set; }

        public Task ExecuteResultAsync(ActionContext context)
        {
            FileResult result;
            if (src.GetType() == typeof(FileStream))
            {
                var o = (FileStream) src;
                result = new FileStreamResult(o, this.ContentType);
            }
            else
            {
                var o = (BinaryDataContext) src;
                result = new FileContentResult(o.Buffer, ContentType);
            }

            result.FileDownloadName = this.DownloadFileName;
            if (!LastModified.Equals(new DateTime(1, 1, 1, 0, 0, 0)))
                result.LastModified = new DateTimeOffset(LastModified); 
            return result.ExecuteResultAsync(context);
        }

        public object UnderlyingObject => this;

        [ScriptConstructor(Name="По имени файла и типу данных")]
        public static FileActionResult ByPhysicalPath(string file, string contentType)
        {
            return new FileActionResult(file, contentType);
        }

        [ScriptConstructor(Name = "По двоичным данным")]
        public static FileActionResult ByBinaryData(BinaryDataContext binaryData, string contentType)
        {
            return new FileActionResult(binaryData, contentType);
        }
    }
}
