using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            var wa = (WebApplicationEngine) _runtime;
            wa?.CodeStatCollector.EndCodeStat();
            var statData = wa?.CodeStatCollector.GetStatData();
            if (statData != null)
            {
                DumpStats(context, statData);
            }
        }

        private void DumpStats(ControllerContext context, CodeStatDataCollection stats)
        {
            var outputFileName = Path.Combine(
                Directory.GetCurrentDirectory(),
                context.ActionDescriptor.ControllerName + Guid.NewGuid().ToString() + ".json");

            using (var w = new StreamWriter(outputFileName))
            {
                var jwriter = new JsonTextWriter(w)
                {
                    Formatting = Formatting.Indented
                };

                jwriter.WriteStartObject();
                foreach (var source in stats.GroupBy(arg => arg.Entry.ScriptFileName))
                {
                    jwriter.WritePropertyName(source.Key, true);
                    jwriter.WriteStartObject();

                    jwriter.WritePropertyName("#path");
                    jwriter.WriteValue(source.Key);
                    foreach (var method in source.GroupBy(arg => arg.Entry.SubName))
                    {
                        jwriter.WritePropertyName(method.Key, true);
                        jwriter.WriteStartObject();

                        foreach (var entry in method.OrderBy(kv => kv.Entry.LineNumber))
                        {
                            jwriter.WritePropertyName(entry.Entry.LineNumber.ToString());
                            jwriter.WriteStartObject();

                            jwriter.WritePropertyName("count");
                            jwriter.WriteValue(entry.ExecutionCount);

                            jwriter.WritePropertyName("time");
                            jwriter.WriteValue(entry.TimeElapsed);

                            jwriter.WriteEndObject();
                        }

                        jwriter.WriteEndObject();
                    }

                    jwriter.WriteEndObject();
                }

                jwriter.WriteEndObject();
                jwriter.Flush();
            }
        }
    }
}
