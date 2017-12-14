using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.Machine;

namespace OneScript.WebHost.Application
{
    public class ApplicationInstance : ScriptDrivenObject
    {
        private static readonly ContextMethodsMapper<ApplicationInstance> OwnMethods = new ContextMethodsMapper<ApplicationInstance>();


        private IApplicationBuilder _startupBuilder;

        public ApplicationInstance(LoadedModuleHandle module): base(module)
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

        private void CallRoutesRegistrationHandler(string handler)
        {
            var handlerIndex = FindMethod(handler);

            var routesCol = new RoutesCollectionContext();

            CallAsProcedure(handlerIndex, new IValue[]{routesCol});

            _startupBuilder.UseMvc(routes =>
            {
                foreach (var route in routesCol)
                {
                    routes.MapRoute(route.Name, route.Template);
                }
            });
        }

        private int FindInternalMethod(string handler)
        {
            // TODO: нет адекватного API для поиска неэкспортного метода (прямо как в ОбработкеОповещения 1С)
            int handlerIndex = -1;
            var mCount = GetMethodsCount();
            for (int i = 0; i < mCount; i++)
            {
                var mi = GetMethodInfo(i);
                if (StringComparer.OrdinalIgnoreCase.Compare(mi.Name, handler) == 0)
                {
                    handlerIndex = i;
                    break;
                }
            }

            if (handlerIndex < 0)
                throw RuntimeException.MethodNotFoundException(handler);
            return handlerIndex;
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
            
            var bc = compiler.CreateModule(src);
            var app = new ApplicationInstance(webApp.Engine.LoadModuleImage(bc));
            webApp.Engine.InitializeSDO(app);

            return app;
        }
    }
}
