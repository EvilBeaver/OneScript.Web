/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using OneScript.WebHost.Application;
using OneScript.WebHost.Database;
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

            var info = (DynamicCompilationInfo) context.ActionDescriptor.Properties["CompilationInfo"];
            var module = info.Module;
            var instance = new ScriptedController(context, module);
            var machine = MachineInstance.Current;
            machine.PrepareThread(engine.Environment);

            _runtime.DebugCurrentThread();
            engine.InitializeSDO(instance);
            return instance;
        }

        public void Release(ControllerContext context, object controller)
        {
            _runtime.StopDebugCurrentThread();
        }
    }
}
