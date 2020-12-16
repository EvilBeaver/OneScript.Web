/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using ScriptEngine.Machine.Contexts;

using ScriptEngine.Machine;

using Hangfire;
using Hangfire.Storage;
using OneScript.WebHost.Database;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;


namespace OneScript.WebHost.BackgroundJobs
{
    [ContextClass("МенеджерРегламентныхЗаданий", "ScheduledJobsManager")]
    public class ScheduledJobsManagerContext : AutoContext<ScheduledJobsManagerContext>
    {
        private static RuntimeEnvironment _globalEnv;
        private static DbContextProvider _dbBridge;

        public ScheduledJobsManagerContext(RuntimeEnvironment env, DbContextProvider dbBridge)
        {
            _globalEnv = env;
            _dbBridge = dbBridge;
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
            
            BackgroundJob.ContinueJobWith(
                TaskIDFrom,
                ()=>PerformAction(module, method));

        }
        
        public static void PerformAction(string module, string method)
        {
            MachineInstance.Current.PrepareThread(_globalEnv);

            ApplicationDbContext dbContext = null;
            try
            {
                dbContext = _dbBridge?.CreateContext();
                
                // TODO Сделать нормальный наконец способ доступа к ИБ
                if (DatabaseExtensions.Infobase != null)
                {
                    DatabaseExtensions.Infobase.DbContext = dbContext;
                }
                
                var scriptObject = (IRuntimeContextInstance) _globalEnv.GetGlobalProperty(module);
                var methodId = scriptObject.FindMethod(method);
                scriptObject.CallAsProcedure(methodId, new IValue[0]);
            }
            finally
            {
                dbContext?.Dispose();
                if (DatabaseExtensions.Infobase != null)
                {
                    DatabaseExtensions.Infobase.DbContext = null;
                }
            }
        }
    }
}
