using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OneScript.WebHost.BackgroundJobs;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Application
{
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

        [ContextMethod("ИспользоватьОбработчикОшибок")]
        public void UseErrorHandler(string errorRoute)
        {
            _startupBuilder.UseExceptionHandler(errorRoute);
        }

        [ContextMethod("ИспользоватьСтатическиеФайлы")]
        public void UseStaticFiles()
        {
            _startupBuilder.UseStaticFiles();
        }

        [ContextMethod("ИспользоватьМаршруты")]
        public void UseMvcRoutes(string handler = null)
        {
            if (handler == null)
                _startupBuilder.UseMvcWithDefaultRoute();
            else
                CallRoutesRegistrationHandler(handler);
        }

        [ContextMethod("ИспользоватьСессии")]
        public void UseSessions()
        {
            _startupBuilder.UseSession();
        }

        [ContextMethod("ИспользоватьАвторизацию")]
        public void UseAuthorization()
        {
            _startupBuilder.UseAuthentication();
        }
        
        [ContextMethod("ИспользоватьФоновыеЗадания")]
        public void UseBackgroundJobs()
        {
            _startupBuilder.UseHangfireServer();
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

            _startupBuilder.UseMvc(routes =>
            {
                foreach (var route in routesCol)
                {
                    routes.MapRoute(route.Name, route.Template, route.Defaults?.Select(x=>
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
            webApp.Environment.LoadMemory(machine);
            webApp.Engine.InitializeSDO(app);

            return app;
        }
    }
}
