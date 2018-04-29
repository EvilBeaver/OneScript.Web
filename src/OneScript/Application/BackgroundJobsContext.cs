using System;
using System.Linq;
using ScriptEngine.Machine.Contexts;

using ScriptEngine.Machine;

using Hangfire;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;

namespace OneScript.WebHost.Application
{
    [ContextClass("МенеджерФоновыхЗаданий", "BackgroundJobsManager")]
    public class BackgroundJobsContext : AutoContext<BackgroundJobsContext>
    {
        private static RuntimeEnvironment _globalEnv;

        public BackgroundJobsContext(RuntimeEnvironment env)
        {
            _globalEnv = env;
        }
        
        [ContextMethod("ВыполнитьЗадание")]
        public void RunTask(string module, string method, ArrayImpl args)
        {
            //todo - а параметры то нужно сериализовать
            
            BackgroundJob.Enqueue(()=>PerformAction(module, method));
        }
        
        [ContextMethod("ВыполнитьЗаданиеОднократно")]
        public void RunTaskOnce(string module, string method, ArrayImpl args)
        {
            //todo - не забыть сделать настройки повторений
            
            BackgroundJob.Enqueue(()=>PerformAction(module, method));
        }
        
        [ContextMethod("ЗапланироватьЗаданиеПоРасписанию")]
        public void RunSheduledTask(string module, string method, ArrayImpl args)
        {
            //todo - вопрос как передавать расписание - cron никому понятен не будет
            
            BackgroundJob.Enqueue(()=>PerformAction(module, method));
        }
        
        [ContextMethod("ЗапланироватьПоследовательность")]
        public void RunSheduledTask(string TaskIDFrom, string module, string method, ArrayImpl args)
        {
            //todo - вопрос как передавать расписание - cron никому понятен не будет
            
            BackgroundJob.Enqueue(()=>PerformAction(module, method));
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