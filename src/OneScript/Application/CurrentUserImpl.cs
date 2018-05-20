using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    [ContextClass("ТекущийПользовательHttp", "CurrentUserHttp")]
    public class CurrentUserImpl : AutoContext<CurrentUserImpl>, IObjectWrapper
    {

        private HttpContext _ctx;

        public CurrentUserImpl(HttpContext ctxHttpContext)
        {
            _ctx = ctxHttpContext;
        }
        
        [ContextMethod("Аутентифировать")]
        public async Task Login(string authType, string redirectUri)
        {
            await _ctx.ChallengeAsync(authType, new AuthenticationProperties() { RedirectUri = redirectUri });   
        }
        
        [ContextMethod("ВыйтиИзСистемы")]
        public async Task Logout(string authType, string redirectUri)
        {
            await _ctx.SignOutAsync(authType, new AuthenticationProperties
            {
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be whitelisted in the 
                // **Allowed Logout URLs** settings for the app.
                RedirectUri = redirectUri
            });
            await _ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
         
        }

        public object UnderlyingObject { get; }
    }
}