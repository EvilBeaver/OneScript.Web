using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OneScript.WebHost.Identity;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Database
{
    public class InfobaseUserContext : AutoContext<InfobaseUserContext>
    {
        public InfobaseUserContext(UserManager<ApplicationUser> getRequiredService)
        {
            throw new NotImplementedException();
        }

        [ContextProperty("Имя")]
        public string Name { get; set; }
    }
}
