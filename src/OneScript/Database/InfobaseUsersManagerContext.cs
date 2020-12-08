/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OneScript.WebHost.Identity;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Database
{
    [ContextClass("МенеджерПользователейИнформационнойБазы", "InfoBaseUsersManager")]
    public class InfobaseUsersManagerContext : AutoContext<InfobaseUsersManagerContext>
    {
        private readonly IHttpContextAccessor _requestAccessor;

        public InfobaseUsersManagerContext(IHttpContextAccessor requestAccessor)
        {
            _requestAccessor = requestAccessor;
        }

        [ContextMethod("ПолучитьПользователей")]
        public ArrayImpl GetUsers()
        {
            var arr = new ArrayImpl();
            var usersManager = _requestAccessor.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var sysUser in usersManager.Users.OrderBy(x=>x.UserName).ToList())
            {
                arr.Add(HydrateUserContext(usersManager, sysUser));
            }

            return arr;
        }

        private static InfobaseUserContext HydrateUserContext(UserManager<ApplicationUser> usersManager, ApplicationUser sysUser)
        {
            return new InfobaseUserContext(usersManager)
            {
                Name = sysUser.UserName,
                StoredPasswordValue = sysUser.PasswordHash,
                UserId = sysUser.Id,
                Email = sysUser.Email
            };
        }

        [ContextMethod("СоздатьПользователя")]
        public InfobaseUserContext CreateUser()
        {
            return new InfobaseUserContext(GetUsersManager())
            {
                IsNew = true
            };

        }

        [ContextMethod("НайтиПоИмени")]
        public IValue FindByName(string name)
        {
            var manager = GetUsersManager();
            var appUser = manager.FindByNameAsync(name).Result;
            if (appUser == null)
                return ValueFactory.Create();

            return HydrateUserContext(manager, appUser);
        }

        [ContextMethod("НайтиПоУникальномуИдентификатору")]
        public IValue FindByUUID(string uuid)
        {
            var manager = GetUsersManager();
            var appUser = manager.FindByIdAsync(uuid).Result;
            if (appUser == null)
                return ValueFactory.Create();

            return HydrateUserContext(manager, appUser);
        }

        private UserManager<ApplicationUser> GetUsersManager()
        {
            return _requestAccessor.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        }

        [ContextMethod("ТекущийПользователь")]
        public IValue CurrentUser()
        {
            var contextObj = _requestAccessor.HttpContext;
            if (contextObj == null)
                return ValueFactory.Create();

            var user = contextObj.User;
            if (user == null)
                return ValueFactory.Create();

            var manager = GetUsersManager();
            var appUser = manager.GetUserAsync(user).Result;
            if (appUser == null)
                return ValueFactory.Create();
            
            return HydrateUserContext(manager, appUser);
        }

        [ContextMethod("АвторизоватьПоПаролю")]
        public bool AuthorizeByPassword(InfobaseUserContext user, string password, bool remember = false)
        {
            if (user == null)
                throw RuntimeException.InvalidArgumentValue();

            var manager = GetUsersManager();
            var appUser = manager.FindByIdAsync(user.UserId).Result;
            if (appUser == null)
                return false;

            var signer = _requestAccessor.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            signer.SignOutAsync().Wait();
            var result = signer.PasswordSignInAsync(appUser, password, remember, false).Result;

            return result.Succeeded;

        }
        
        [ContextMethod("СброситьАутентификацию")]
        public bool ResetAuthorization()
        {
            var signer = _requestAccessor.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            
            return signer.SignOutAsync().IsCompleted;

        }
        
    }
}
