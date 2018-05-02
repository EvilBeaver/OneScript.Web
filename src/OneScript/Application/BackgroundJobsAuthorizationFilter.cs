using System.Collections.Generic;
using Hangfire.Dashboard;

namespace OneScript.WebHost.Application
{
    public class BackgroundJobsAuthorizationFilter : IAuthorizationFilter
    {
        private bool grantAnonymous = false; 
        
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {

            if (grantAnonymous)
            {
                return true;
            }
            else
            {
                throw new System.NotImplementedException();     
            }
            
        }

        public BackgroundJobsAuthorizationFilter(bool isGrantAnonymous = false)
        {
            grantAnonymous = isGrantAnonymous;
        }
    }
}