/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using Hangfire;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.BackgroundJobs
{
    [ContextClass("МенеджерФоновыхЗаданий", "BackgroundJobsManager")]
    public class BackgroundJobsManagerContext : AutoContext<BackgroundJobsManagerContext>
    {
        private static RuntimeEnvironment _globalEnv;

        public BackgroundJobsManagerContext(RuntimeEnvironment env)
        {
            _globalEnv = env;
        }

        /// <summary>
        /// Выполняет заданный метод в фоновом режиме. Метод должен располагаться в общем модуле и быть экспортным.
        /// Параметры метода в текущей версии не поддерживаются.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        [ContextMethod("Выполнить", "Execute")]
        public BackgroundJobContext Execute(string method)
        {
            var callAddr = method.Split(new[]{'.'}, 2);
            if (callAddr.Length != 2)
                throw RuntimeException.InvalidArgumentValue(method);

            var jobId = BackgroundJob.Enqueue(
                () => PerformAction(callAddr[0], callAddr[1])
            );

            return new BackgroundJobContext(jobId);
        }

        [ContextMethod("ОжидатьЗавершения")]
        public void WaitForCompletion()
        {
            throw new NotImplementedException();
        }

        // должен быть public, т.к. hangfire не умеет вызывать private
        public static void PerformAction(string module, string method)
        {
            MachineInstance.Current.PrepareThread(_globalEnv);

            var scriptObject = (IRuntimeContextInstance)_globalEnv.GetGlobalProperty(module);
            var methodId = scriptObject.FindMethod(method);
            scriptObject.CallAsProcedure(methodId, new IValue[0]);
        }
    }
}
