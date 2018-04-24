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