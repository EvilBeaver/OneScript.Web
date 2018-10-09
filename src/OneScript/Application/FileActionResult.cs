using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
