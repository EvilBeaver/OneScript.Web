/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using Microsoft.AspNetCore.Identity;
using OneScript.WebHost.Identity;
using ScriptEngine.HostedScript.Library;
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

        [ContextProperty("ЭлектроннаяПочта", "Email")]
        public string Email { get; set; }


        [ContextMethod("Записать", "Write")]
        public void Write()
        {
            IdentityResult result;
            
            if (IsNew)
            {
                var appUser = new ApplicationUser
                {
                    UserName = Name,
                    Email = Email
                };
                if(String.IsNullOrEmpty(Password))
                    result = _userService.CreateAsync(appUser).Result;
                else
                    result = _userService.CreateAsync(appUser, Password).Result;

                if (result.Succeeded)
                    UserId = appUser.Id;
            }
            else
            {
                var appUser = _userService.FindByIdAsync(UserId).Result;
                if (appUser == null)
                    throw new RuntimeException("Current user ID isn't in database");

                appUser.UserName = Name;
                appUser.Email = Email;
                appUser.PasswordHash = _userService.PasswordHasher.HashPassword(appUser, Password);

                result = _userService.UpdateAsync(appUser).Result;
            }
            
            if (!result.Succeeded)
            {
                var resultArr = new ArrayImpl();
                foreach (var identityError in result.Errors)
                {
                    var strValue = ValueFactory.Create($"{{{identityError.Code}}} - {identityError.Description}");
                    resultArr.Add(strValue);
                }

                throw new ParametrizedRuntimeException("Ошибка создания пользователя", resultArr);
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

            if (!result.Succeeded)
            {
                var resultArr = new ArrayImpl();
                foreach (var identityError in result.Errors)
                {
                    var strValue = ValueFactory.Create($"{{{identityError.Code}}} - {identityError.Description}");
                    resultArr.Add(strValue);
                }

                throw new ParametrizedRuntimeException("Ошибка удаления пользователя", resultArr);
            }
        }
    }
}
