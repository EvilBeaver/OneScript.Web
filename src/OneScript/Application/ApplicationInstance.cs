using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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

        internal void OnControllersCreation(out IEnumerable<string> files, ref bool standardHandling)
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

        internal void OnViewComponentsCreation(out IEnumerable<string> files, ref bool standardHandling)
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
        
        [ContextMethod("ИспользоватьКонсольЗаданий")]
        public void UseBackgroudDashboard(string routeforjobs = "/jobs")
        {
            if (routeforjobs == null) {
                throw RuntimeException.InvalidArgumentValue("Неопределён маршрут для консоли заданий");
            } else
            {
               _startupBuilder.UseHangfireDashboard(routeforjobs);    
            }
        }

        [ContextMethod("ИспользоватьВнешнююАутентификацию")]
        public void UseExternalAuth(string serviceID, StructureImpl authParams)
        {
            switch (serviceID)
            {
                case "auth0":
                    ConfigureAuth0(authParams);
                    break;
                default:
                    throw RuntimeException.InvalidArgumentValue("Неизвестный сервис аутентификации " + serviceID);
            }
            
        }

        private void ConfigureAuth0(StructureImpl authParams)
        {

            var _appDomain = ""; 
            var _appCID = "";
            var _appCIS = "";
            
            //todo убрать это странный копипаст
            if (authParams.HasProperty("Domain")) { _appDomain = authParams.GetPropValue(authParams.FindProperty("Domain")).AsString(); }
            else { throw RuntimeException.InvalidArgumentValue("Для сервиса auth0 не определено свойство Domain в структуре параметров"); };
            
            if (authParams.HasProperty("ClientId")) { _appCID = authParams.GetPropValue(authParams.FindProperty("ClientId")).AsString(); }
            else { throw RuntimeException.InvalidArgumentValue("Для сервиса auth0 не определено свойство ClientId в структуре параметров"); };
            
            if (authParams.HasProperty("ClientSecret")) { _appCIS = authParams.GetPropValue(authParams.FindProperty("ClientSecret")).AsString(); }
            else { throw RuntimeException.InvalidArgumentValue("Для сервиса auth0 не определено свойство ClientSecret в структуре параметров"); };


            var AuthService = _startupBuilder.ApplicationServices.GetService<AuthenticationBuilder>();
            
            AuthService.AddCookie()
                .AddOpenIdConnect("Auth0", options =>
                {
                    // Set the authority to your Auth0 domain
                    options.Authority = $"https://{_appDomain}";

                    // Configure the Auth0 Client ID and Client Secret
                    options.ClientId = _appCID;
                    options.ClientSecret = _appCIS;

                    // Set response type to code
                    options.ResponseType = "code";

                    // Configure the scope
                    options.Scope.Clear();
                    options.Scope.Add("openid");

                    // Set the callback path, so Auth0 will call back to http://localhost:5000/signin-auth0 
                    // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard 
                    options.CallbackPath =
                        new PathString(
                            "/signin-auth0"); //todo - позволить пользователю самому определять контролер аутентификации

                    // Configure the Claims Issuer to be Auth0
                    options.ClaimsIssuer = "Auth0";

                    options.Events = new OpenIdConnectEvents
                    {
                        // handle the logout redirection 
                        OnRedirectToIdentityProviderForSignOut = (context) =>
                        {
                            var logoutUri = $"https://{_appDomain}/v2/logout?client_id={_appCID}";

                            var postLogoutUri = context.Properties.RedirectUri;
                            if (!string.IsNullOrEmpty(postLogoutUri))
                            {
                                if (postLogoutUri.StartsWith("/"))
                                {
                                    // transform to absolute
                                    var request = context.Request;
                                    postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase +
                                                    postLogoutUri;
                                }
                                logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                            }

                            context.Response.Redirect(logoutUri);
                            context.HandleResponse();

                            return Task.CompletedTask;
                        }
                    };
                });
            
                _startupBuilder.UseAuthentication();

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
            
            
            webApp.Engine.InitializeSDO(app);

            return app;
        }
    }
}
