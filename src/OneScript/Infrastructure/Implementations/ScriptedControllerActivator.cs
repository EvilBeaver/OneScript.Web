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
        private IApplicationRuntime _runtime;
        private ApplicationDbContext _dbContext;

        public ScriptedControllerActivator(IApplicationRuntime app) : this (app, null)
        { 
        }

        public ScriptedControllerActivator(IApplicationRuntime app, ApplicationDbContext dbContext)
        {
            _runtime = app;
            _dbContext = dbContext;
        }

        public object Create(ControllerContext context)
        {
            var engine = _runtime.Engine;
            if (DatabaseExtensions.Infobase != null)
            {
                DatabaseExtensions.Infobase.DbContext = _dbContext;
            }
            var instance = new ScriptedController(context, (LoadedModule)context.ActionDescriptor.Properties["module"]);
            var machine = MachineInstance.Current;
            engine.Environment.LoadMemory(machine);
            engine.InitializeSDO(instance);
            return instance;
        }

        public void Release(ControllerContext context, object controller)
        {
            //throw new NotImplementedException();
        }
    }
}
