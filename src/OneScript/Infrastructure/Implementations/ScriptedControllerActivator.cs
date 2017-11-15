using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class ScriptedControllerActivator : IControllerActivator
    {
        public object Create(ControllerContext context)
        {
            throw new NotImplementedException();
        }

        public void Release(ControllerContext context, object controller)
        {
            throw new NotImplementedException();
        }
    }
}
