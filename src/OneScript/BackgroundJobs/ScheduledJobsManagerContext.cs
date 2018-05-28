using System;
using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Machine.Contexts;

using ScriptEngine.Machine;

using Hangfire;
using Hangfire.Storage;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;


namespace OneScript.WebHost.BackgroundJobs
{
    [ContextClass("МенеджерРегламентныхЗаданий", "ScheduledJobsManager")]
    public class ScheduledJobsManagerContext : AutoContext<ScheduledJobsManagerContext>
    {
        private static RuntimeEnvironment _globalEnv;

        public ScheduledJobsManagerContext(RuntimeEnvironment env)
        {
            _globalEnv = env;
        }
        
        [ContextMethod("ВыполнитьОтложенноеЗадание")]
        public string RunSheduledTask(string module, string method, TimeSpanWrapper sheduler)
        {

            var jobId = BackgroundJob.Schedule(
                ()=>PerformAction(module, method), 
                sheduler.GetShuller());
            return jobId;
        }
        
        [ContextMethod("СоздатьПериодическоеЗаданиеПоРасписанию")]
        public void CreateRecurringSheduledTask(string module, string method, CronWrapper cron)
        {
            
            RecurringJob.AddOrUpdate(
                ()=>PerformAction(module, method),
                cron.CronString);

        }
        
        [ContextMethod("УдалитьПериодическоеЗаданиеПоРасписанию")]
        public void RemoveRecurringSheduledTask(string taskId)
        {
            
            RecurringJob.RemoveIfExists(taskId);

        }
        
        [ContextMethod("ВыполнитьПринудительноПериодическоеЗаданиеПоРасписанию")]
        public void TriggerRecurringSheduledTask(string taskId)
        {
            
            RecurringJob.Trigger(taskId);

        }
        
        
        [ContextMethod("ПолучитьИдентификаторыПериодическихЗаданий")]
        public ArrayImpl GetRecurringJobsIDs()
        {
            
            List<RecurringJobDto> recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            
            var arr = new ArrayImpl();

            foreach (var recurringJob in recurringJobs)
            {
                arr.Add(ValueFactory.Create(recurringJob.Id));
            }

            return arr;
        }

        
        [ContextMethod("ВыполнитьПодчиненноеЗадание")]
        public void RunContinuationsTask(string TaskIDFrom, string module, string method)
        {
            
            BackgroundJob.ContinueWith(
                TaskIDFrom,
                ()=>PerformAction(module, method));

        }
        
        public static void PerformAction(string module, string method)
        {
            _globalEnv.LoadMemory(MachineInstance.Current);
            
            var scriptObject = (IRuntimeContextInstance) _globalEnv.GetGlobalProperty(module);
            var methodId = scriptObject.FindMethod(method);
            scriptObject.CallAsProcedure(methodId, new IValue[0]);
        }
    }
}