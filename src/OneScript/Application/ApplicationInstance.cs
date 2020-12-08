/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using OneScript.WebHost.BackgroundJobs;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Application
{
    /// <summary>
    ////Экземпляр серверного приложения. Представлен модулем приложения main.os
    /// </summary>
    [ContextClass("Приложение")]
    public class ApplicationInstance : ScriptDrivenObject
    {
        private static readonly ContextMethodsMapper<ApplicationInstance> OwnMethods = new ContextMethodsMapper<ApplicationInstance>();


        private IApplicationBuilder _startupBuilder;
        public ApplicationInstance(LoadedModule module): base(module)
        {
            
        }

        protected override int GetOwnMethodCount()
        {
            return OwnMethods.Count;
        }

        protected override int GetOwnVariableCount()
        {
            return 0;
        }

        protected override void UpdateState()
        {
            
        }

        protected override int FindOwnMethod(string name)
        {
            try
            {
                return OwnMethods.FindMethod(name);
            }
            catch (RuntimeException)
            {
                return -1;
            }
        }

        public void OnControllersCreation(out IEnumerable<string> files, ref bool standardHandling)
        {
            var mId = GetScriptMethod("ПриРегистрацииКонтроллеров");
            if (mId == -1)
            {
                files = null;
                return;
            }

            var boolValue = ValueFactory.Create(standardHandling);
            var boolReference = Variable.Create(boolValue, "");
            var parameters = new IValue[]{new ArrayImpl(), boolReference};
            CallScriptMethod(mId, parameters);

            var arr = parameters[0].AsObject() as ArrayImpl;
            if (arr == null)
                throw RuntimeException.InvalidArgumentType();

            files = arr.Select(x => x.AsString());
            standardHandling = parameters[1].AsBoolean();
        }

        public void OnViewComponentsCreation(out IEnumerable<string> files, ref bool standardHandling)
        {
            var mId = GetScriptMethod("ПриРегистрацииКомпонентовПредставлений");
            if (mId == -1)
            {
                files = null;
                return;
            }

            var boolValue = ValueFactory.Create(standardHandling);
            var boolReference = Variable.Create(boolValue, "");
            var parameters = new IValue[] { new ArrayImpl(), boolReference };
            CallScriptMethod(mId, parameters);

            var arr = parameters[0].AsObject() as ArrayImpl;
            if (arr == null)
                throw RuntimeException.InvalidArgumentType();

            files = arr.Select(x => x.AsString());
            standardHandling = parameters[1].AsBoolean();
        }

        protected override MethodInfo GetOwnMethod(int index)
        {
            return OwnMethods.GetMethodInfo(index);
        }

        protected override void CallOwnProcedure(int index, IValue[] arguments)
        {
            OwnMethods.GetMethod(index)(this, arguments);
        }

        protected override IValue CallOwnFunction(int index, IValue[] arguments)
        {
            return OwnMethods.GetMethod(index)(this, arguments);
        }

        /// <summary>
        /// Добавляет компонент конвейера, отвечающий за обработку исключений
        /// </summary>
        /// <param name="errorRoute">Маршрут URL, который будет отображаться при возникновении исключения</param>
        [ContextMethod("ИспользоватьОбработчикОшибок")]
        public void UseErrorHandler(string errorRoute)
        {
            _startupBuilder.UseExceptionHandler(errorRoute);
        }

        /// <summary>
        /// Добавляет компонент конвейера, отвечающий за выдачу статического содержимого (картинок, скриптов, стилей и т.п.)
        /// </summary>
        [ContextMethod("ИспользоватьСтатическиеФайлы")]
        public void UseStaticFiles()
        {
            _startupBuilder.UseStaticFiles();
        }

        /// <summary>
        /// Добавляет компонент конвейера, отвечающий за обработку MVC-маршрутов, контроллеры и представления.
        /// По умолчанию добавляется маршрут /{controller=home}/{action=index}/{id?}.
        /// В метод можно передать имя процедуры-обработчика, в которой можно будет перенастроить шаблоны URL.
        /// </summary>
        /// <param name="handler">Имя процедуры-обработчика, в которой будет настраиваться маршрутизация.</param>
        [ContextMethod("ИспользоватьМаршруты")]
        public void UseMvcRoutes(string handler = null)
        {
            _startupBuilder.UseRouting();
            if (handler == default)
            {
                _startupBuilder.UseEndpoints(x => x.MapDefaultControllerRoute());
            }
            else
            {
                CallRoutesRegistrationHandler(handler);
            }
        }

        /// <summary>
        /// Добавляет middleware в конвейер.
        /// В метод нужно передать имя файла скрипта относительно /app, в котором лежит обработчик middleware.
        /// </summary>
        /// <param name="scriptName">Имя скрипта-обработчика, который будет вызываться.</param>
        [ContextMethod("ИспользоватьПосредника")]
        public void UseMiddleware(string scriptName)
        {
            _startupBuilder.UseScriptedMiddleware(scriptName);
        }

        /// <summary>
        /// Использовать обработчик cookies, отвечающих за клиентские сессии. Позволяет применять http-сессии в контроллерах
        /// </summary>
        [ContextMethod("ИспользоватьСессии")]
        public void UseSessions()
        {
            _startupBuilder.UseSession();
        }

        /// <summary>
        /// Использовать обработчик cookies, отвечающих за клиентскую аутентификацию.
        /// </summary>
        [ContextMethod("ИспользоватьАвторизацию")]
        public void UseAuthorization()
        {
            _startupBuilder.UseAuthentication();
        }

        /// <summary>
        /// Разрешает использование фоновых и регламентных заданий. Запускает сервер обслуживания заданий Hangfire.
        /// </summary>
        [ContextMethod("ИспользоватьФоновыеЗадания")]
        public void UseBackgroundJobs()
        {
            _startupBuilder.UseHangfireServer();
        }

        [ContextMethod("ИспользоватьСжатиеОтветов")]
        public void UseResponseCompression()
        {
            _startupBuilder.UseResponseCompression();
        }

        // TODO:
        // Включить управление консолью, когда будет готова архитектура ролей в целом.
        //[ContextMethod("ИспользоватьКонсольЗаданий")]
        public void UseBackgroundDashboard(string routeforjobs = "/jobs")
        {

            if (routeforjobs == "")
            {
                throw RuntimeException.InvalidArgumentValue("Please provide route for jobs console");
            }

            _startupBuilder.UseHangfireDashboard(routeforjobs, new DashboardOptions
            {
                Authorization = new[] { new DashboardAutorizationFilter() } //fixme - нужна еще и роль пользователя
            });

        }


        private void CallRoutesRegistrationHandler(string handler)
        {
            var handlerIndex = GetScriptMethod(handler);

            var routesCol = new RoutesCollectionContext();
            CallScriptMethod(handlerIndex, new IValue[]{routesCol});

            _startupBuilder.UseEndpoints(routes =>
            {
                foreach (var route in routesCol)
                {
                    routes.MapControllerRoute(route.Name, route.Template, route.Defaults?.Select(x=>
                    {
                        var kv = new KeyValuePair<string,object>(x.Key.AsString(),ContextValuesMarshaller.ConvertToCLRObject(x.Value));
                        return kv;
                    }));
                }
            });
        }
        
        public void OnStartup(IApplicationBuilder aspAppBuilder)
        {
            int startup = GetScriptMethod("ПриНачалеРаботыСистемы", "OnSystemStartup");
            if(startup == -1)
                return;

            _startupBuilder = aspAppBuilder;

            CallScriptMethod(startup, new IValue[] { });
        }

        public static ApplicationInstance Create(ICodeSource src, IApplicationRuntime webApp)
        {
            var compiler = webApp.Engine.GetCompilerService();

            for (int i = 0; i < OwnMethods.Count; i++)
            {
                compiler.DefineMethod(OwnMethods.GetMethodInfo(i));
            }
            
            var bc = compiler.Compile(src);
            var app = new ApplicationInstance(new LoadedModule(bc));
            var machine = MachineInstance.Current;
            machine.PrepareThread(webApp.Environment);
            webApp.Engine.InitializeSDO(app);

            return app;
        }
    }
}
