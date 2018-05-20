using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
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
        private IServiceCollection _serviceConfig;

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
        
        [ContextMethod("ИспользоватьАутентификацию")]
        public void UseAuthentication()
        {
            _startupBuilder.UseAuthentication();
        }
        
        [ContextMethod("ДобавитьАутентификациюAuth0")]
        public void AddAuthenticationAuth0(string Domain, string ClientId, string ClientSecret)
        {
            
            _serviceConfig.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(
                    "Auth0", options =>
                    {
                        // Set the authority to your Auth0 domain
                        options.Authority = $"https://{Domain}";
                        options.ClientId = ClientId;
                        options.ClientSecret = ClientSecret;
                        
                        options.ResponseType = "code";
                        
                        options.Scope.Clear();
                        options.Scope.Add("openid");
                        
                        options.CallbackPath = new PathString("/signin-auth0");
                        
                        options.ClaimsIssuer = "Auth0";

                        options.Events = new OpenIdConnectEvents
                        {
                            // handle the logout redirection 
                            OnRedirectToIdentityProviderForSignOut = (context) =>
                            {
                                var logoutUri = $"https://{Domain}/v2/logout?client_id={ClientId}";

                                var postLogoutUri = context.Properties.RedirectUri;
                                if (!string.IsNullOrEmpty(postLogoutUri))
                                {
                                    if (postLogoutUri.StartsWith("/"))
                                    {
                                        // transform to absolute
                                        var request = context.Request;
                                        postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                                    }
                                    logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                                }

                                context.Response.Redirect(logoutUri);
                                context.HandleResponse();

                                return Task.CompletedTask;
                            }
                        };

                    })
                
                ;
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

        public void Configure(IServiceCollection services)
        {
            int configure = GetScriptMethod("ПриНачалеНастройкиСистемы", "OnSystemConfigure");
            if(configure == -1)
                return;

            _serviceConfig = services;
            
            CallScriptMethod(configure, new IValue[] { });

        }
        
        public void OnStartup(IApplicationBuilder aspAppBuilder)
        {
            int startup = GetScriptMethod("ПриНачалеРаботыСистемы", "OnSystemStartup");
            if(startup == -1)
                return;

            _startupBuilder = aspAppBuilder;

            CallScriptMethod(startup, new IValue[] { });
        }

        public static ApplicationInstance Create(ICodeSource src, IApplicationRuntime webApp, IServiceCollection services)
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

            app.Configure(services);
            
            return app;
        }
    }
}
