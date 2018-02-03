using OneScript.WebHost.Infrastructure;
using ScriptEngine.Environment;

namespace WebhostTests
{
    internal class FakeScriptsProvider : IScriptsProvider
    {
        public FakeScriptsProvider()
        {
           
        }

        public void Add(string virtualPath, string content)
        {

        }

        public ICodeSource Get(string virtualPath)
        {
            throw new System.NotImplementedException();
        }
    }
}