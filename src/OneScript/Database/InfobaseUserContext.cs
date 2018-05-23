using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OneScript.WebHost.Identity;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Database
{
    public class InfobaseUserContext : AutoContext<InfobaseUserContext>
    {
        private readonly UserManager<ApplicationUser>  _userService;
        public InfobaseUserContext(UserManager<ApplicationUser> userService)
        {
            _userService = userService;
        }

        public bool IsNew { get; set; }

        [ContextProperty("Имя")]
        public string Name { get; set; }

        [ContextMethod("Записать")]
        public void Write()
        {
            var appUser = new ApplicationUser();
            appUser.UserName = Name;
            IdentityResult result;
            if (IsNew)
                result = _userService.CreateAsync(appUser).Result;
            else
                result = _userService.UpdateAsync(appUser).Result;

            if (!result.Succeeded)
            {
                var s = result.Errors.ToString();
                throw new RuntimeException(s);
            }
        }
    }
}
