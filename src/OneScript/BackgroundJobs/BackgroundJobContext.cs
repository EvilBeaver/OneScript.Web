using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.BackgroundJobs
{
    [ContextClass("ФоновоеЗадание","BackgroundJob")]
    public class BackgroundJobContext : AutoContext<BackgroundJobContext>
    {
        private string _jobId;

        public BackgroundJobContext(string jobId)
        {
            _jobId = jobId;
        }
    }
}