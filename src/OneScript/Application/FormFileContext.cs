using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("ФайлФормы")]
    public class FormFileContext : AutoContext<FormFileContext>
    {
        private readonly IFormFile _realObject;

        public FormFileContext(IFormFile realObject)
        {
            _realObject = realObject;
        }

        
    }
}
