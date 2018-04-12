using ScriptEngine.Machine.Contexts;
using Hangfire;

namespace OneScript.WebHost.Application
{
    [ContextClass("ФоновыеЗадания", "BackgroundJobs")]
    public class BackgroundJobsContext : AutoContext<BackgroundJobsContext>
    {
        [ContextMethod("ВыполнитьЗадание")]
        public void RunTask(string controller, string method)
        {
            
            //TODO изменить поведение
            var jobId = BackgroundJob.Enqueue(
                () => System.Console.WriteLine("works"));
        }
        
    }
}