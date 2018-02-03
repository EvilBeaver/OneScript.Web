using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class ScriptedControllerActivator : IControllerActivator
    {
        private ScriptingEngine _engine;
        public ScriptedControllerActivator(IApplicationRuntime app)
        {
            _engine = app.Engine;
        }

        public object Create(ControllerContext context)
        {
            var instance = new ScriptedController(context, (LoadedModuleHandle)context.ActionDescriptor.Properties["module"]);
            var machine = MachineInstance.Current;
            _engine.Environment.LoadMemory(machine);
            _engine.InitializeSDO(instance);
            return instance;
        }

        public void Release(ControllerContext context, object controller)
        {
            //throw new NotImplementedException();
        }
    }
}
