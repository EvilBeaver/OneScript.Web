using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Security.Cryptography;

namespace OneScript.WebHost.Application
{
    [ContextClass("РезультатДействияВнешнийВызов")]
    public class ChallengeActionResult : AutoContext<ChallengeActionResult>, IObjectWrapper
    {
        public ChallengeActionResult(ChallengeResult result)
        {
            UnderlyingObject = result;
        }


        [ScriptConstructor]
        public static ChallengeActionResult Constructor(IValue externalAuth, IValue redirectUri)
        {
            var challengeResult = new ChallengeResult(
                externalAuth.AsString(), new AuthenticationProperties() { RedirectUri = redirectUri.AsString() }    
            );
            
            return new ChallengeActionResult(challengeResult);
        }
        
        public object UnderlyingObject { get; }
    }
}