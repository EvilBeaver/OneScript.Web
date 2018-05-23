using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneScript.WebHost.Identity;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Database
{
    [ContextClass("МенеджерПользователейИнформационнойБазы", "InfoBaseUsersManager")]
    public class InfobaseUsersManagerContext : AutoContext<InfobaseUsersManagerContext>
    {
        private readonly IServiceProvider _services;

        public InfobaseUsersManagerContext(IServiceProvider services)
        {
            _services = services;
        }

        [ContextMethod("ПолучитьПользователей")]
        public ArrayImpl GetUsers()
        {
            var arr = new ArrayImpl();
            var usersManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var sysUser in usersManager.Users.ToList())
            {
                arr.Add(new InfobaseUserContext(usersManager)
                {
                    Name = sysUser.UserName
                });
            }

            return arr;
        }

        [ContextMethod("СоздатьПользователя")]
        public InfobaseUserContext CreateUser()
        {
            return new InfobaseUserContext(_services.GetRequiredService<UserManager<ApplicationUser>>())
            {
                IsNew = true
            };

        }
    }
}
