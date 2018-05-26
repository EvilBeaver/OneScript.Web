using System;
using Hangfire;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.BackgroundJobs
{
    [ContextClass("РасписаниеФоновыхЗаданий")]
    public class CronWrapper : AutoContext<CronWrapper>
    {
        private string cronString;
        
        [ContextMethod("КаждыйДень")]
        public void Daily()
        {
            cronString = Cron.Daily();
        }
        
        [ContextMethod("КаждыйДеньВИнтервале")]
        public void DayInterval(int interval)
        {
            cronString = Cron.DayInterval(interval);
        }
        
        [ContextMethod("КаждыйЧас")]
        public void Hourly()
        {
            cronString = Cron.Hourly();
        }
        
        [ContextMethod("КаждыйЧасВИнтервале")]
        public void HourInterval(int interval)
        {
            cronString = Cron.HourInterval(interval);
        }
        
        [ContextMethod("КаждуюМинуту")]
        public void Minutely()
        {
            cronString = Cron.Minutely();
        }
        
        [ContextMethod("КаждуюМинутуВИнтервале")]
        public void MinuteInterval(int interval)
        {
            cronString = Cron.MinuteInterval(interval);
            
        }
        
        [ContextMethod("КаждыйМесяц")]
        public void Monthly()
        {
            cronString = Cron.Monthly();

        }
        
        [ContextMethod("КаждыйГод")]
        public void Yearly()
        {
            cronString = Cron.Yearly();

        }
        
        [ContextMethod("КаждуюНеделю")]
        public void Weekly()
        {
            cronString = Cron.Weekly();

        }
        
        
        [ContextProperty("РасписаниеСтрокой")]
        public string CronString
        {
            get => cronString;
            set => cronString = value; //fixme не уверен что нужно давать 1С-нику прямое управление cron строкой
        }
        
        
        [ScriptConstructor]
        public static CronWrapper Constructor()
        {
            return new CronWrapper();
        }
       
        
    }
}