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
    [ContextClass("ПользовательИнформационнойБазы","InfobaseUser")]
    public class InfobaseUserContext : AutoContext<InfobaseUserContext>
    {
        private readonly UserManager<ApplicationUser>  _userService;
        public InfobaseUserContext(UserManager<ApplicationUser> userService)
        {
            _userService = userService;
        }

        public bool IsNew { get; set; }

        [ContextProperty("УникальныйИдентификатор","UUID")]
        public string UserId { get; set; }

        [ContextProperty("Имя", "Name")]
        public string Name { get; set; }

        [ContextProperty("СохраняемоеЗначениеПароля", "StoredPasswordValue", CanWrite = false)]
        public string StoredPasswordValue { get; set; }

        [ContextProperty("Пароль", "Password")]
        public string Password { get; set; }
        
        [ContextMethod("Записать", "Write")]
        public void Write()
        {
            IdentityResult result;
            
            if (IsNew)
            {
                var appUser = new ApplicationUser();
                appUser.UserName = Name;
                result = _userService.CreateAsync(appUser, Password).Result;
            }
            else
            {
                var appUser = _userService.FindByIdAsync(UserId).Result;
                if (appUser == null)
                    throw new RuntimeException("Current user ID isn't in database");

                appUser.UserName = Name;
                appUser.PasswordHash = _userService.PasswordHasher.HashPassword(appUser, Password);

                result = _userService.UpdateAsync(appUser).Result;
            }
            
            if (!result.Succeeded)
            {
                var s = result.ToString();
                throw new RuntimeException(s);
            }
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete()
        {
            if (IsNew)
            {
                return;
            }

            var appUser = _userService.FindByIdAsync(UserId).Result;
            if (appUser == null)
                throw new RuntimeException("Current user ID isn't in database");

            var result = _userService.DeleteAsync(appUser).Result;

            if (result.Succeeded) return;
            var s = result.ToString();
            throw new RuntimeException(s);
        }
    }
}
