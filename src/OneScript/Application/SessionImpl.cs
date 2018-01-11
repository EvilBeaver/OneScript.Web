using System.Linq;
using Microsoft.AspNetCore.Http;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Infrastructure
{
    [ContextClass("СессияHttp", "HttpSession")]
    public class SessionImpl : AutoContext<SessionImpl>, IObjectWrapper
    {
        private ISession _requestSession;

        public SessionImpl(ISession requestSession)
        {
            _requestSession = requestSession;
        }

        [ContextProperty("Доступна")]
        public bool IsAvailable => _requestSession.IsAvailable;

        [ContextProperty("Идентификатор")]
        public string Identifier => _requestSession.Id;

        [ContextMethod("ПолучитьКлючи")]
        public ArrayImpl GetKeys()
        {
            var arr = new ArrayImpl(_requestSession.Keys.Select(ValueFactory.Create));
            return arr;
        }

        [ContextMethod("ПолучитьСтроку")]
        public IValue GetString(string key)
        {
            var str = _requestSession.GetString(key);
            return str == null ? ValueFactory.Create() : ValueFactory.Create(str);
        }

        [ContextMethod("УстановитьСтроку")]
        public void SetString(string key, string value)
        {
            _requestSession.SetString(key, value);
        }

        [ContextMethod("ПолучитьЧисло")]
        public IValue GetNumber(string key)
        {
            var num = _requestSession.GetInt32(key);
            return num == null ? ValueFactory.Create() : ValueFactory.Create((decimal) num.Value);
        }

        [ContextMethod("УстановитьЧисло")]
        public void SetNumber(string key, int value)
        {
            _requestSession.SetInt32(key, value);
        }

        [ContextMethod("Очистить")]
        public void Clear()
        {
            _requestSession.Clear();
        }

        [ContextMethod("Удалить")]
        public void Remove(string key)
        {
            _requestSession.Remove(key);
        }

        public object UnderlyingObject => _requestSession;
    }
}