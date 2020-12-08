/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.BackgroundJobs
{
    [ContextClass("ПараметрыОжиданияФоновыхЗаданий")]
    public class TimeSpanWrapper : AutoContext<TimeSpanWrapper>
    {
        private TimeSpan elapsedTime = TimeSpan.Zero;

        
        [ContextMethod("Ожидание")]
        public void TimeSpanFrom(int days, int hours, int minutes, int seconds, int millisec )
        {
            elapsedTime = new TimeSpan(
                days, hours, minutes, seconds, millisec);
        }
        
        
        [ContextMethod("ОжиданиеВДнях")]
        public void TimeSpanFromDays(int days)
        {
            elapsedTime = TimeSpan.FromDays(days);
        }
        
        [ContextMethod("ОжиданиеВЧасах")]
        public void TimeSpanFromHours(int hours)
        {
            elapsedTime = TimeSpan.FromHours(hours);
        }
        
        [ContextMethod("ОжиданиеВМинутах")]
        public void TimeSpanFromMinutes(int minutes)
        {
            elapsedTime = TimeSpan.FromMinutes(minutes);
        }
        
        [ContextMethod("ОжиданиеВСекундах")]
        public void TimeSpanFromSecconds(int seconds)
        {
            elapsedTime = TimeSpan.FromSeconds(seconds);
        }
        
        [ContextMethod("ОжиданиеВМиллисекундах")]
        public void TimeSpanFromSMilliseconds(int milliseconds)
        {
            elapsedTime = TimeSpan.FromMilliseconds(milliseconds);
        }

        public TimeSpan GetShuller()
        {
            return elapsedTime;
        }
        
        [ScriptConstructor]
        public static TimeSpanWrapper Constructor()
        {
            return new TimeSpanWrapper();
        }
        
    }
}
