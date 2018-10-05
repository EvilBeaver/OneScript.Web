using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneScript.WebHost.Application;
using OneScript.WebHost.Database;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure.Implementations
{
    public class ScriptedControllerActivator : IControllerActivator
    {
        private ScriptingEngine _engine;
        private ApplicationDbContext _dbContext;

        public ScriptedControllerActivator(IApplicationRuntime app, ApplicationDbContext dbContext)
        {
            _engine = app.Engine;
            _dbContext = dbContext;
        }

        public object Create(ControllerContext context)
        {
            var instance = new ScriptedController(context, (LoadedModule)context.ActionDescriptor.Properties["module"]);
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
