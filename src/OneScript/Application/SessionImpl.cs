/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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

        /// <summary>
        /// Возвращает Истина, если пользовательская сессия была инициирована
        /// </summary>
        [ContextProperty("Доступна")]
        public bool IsAvailable => _requestSession.IsAvailable;

        /// <summary>
        /// Идентификатор сессии
        /// </summary>
        [ContextProperty("Идентификатор")]
        public string Identifier => _requestSession.Id;

        /// <summary>
        /// Метод получает набор ключей, сохраненных в сессии
        /// </summary>
        /// <returns>Массив ключей</returns>
        [ContextMethod("ПолучитьКлючи")]
        public ArrayImpl GetKeys()
        {
            var arr = new ArrayImpl(_requestSession.Keys.Select(ValueFactory.Create));
            return arr;
        }

        /// <summary>
        /// Получить строковое значение из сессии
        /// </summary>
        /// <param name="key">Ключ значения</param>
        /// <returns></returns>
        [ContextMethod("ПолучитьСтроку")]
        public IValue GetString(string key)
        {
            var str = _requestSession.GetString(key);
            return str == null ? ValueFactory.Create() : ValueFactory.Create(str);
        }

        /// <summary>
        /// Установить строковое значение в сессию
        /// </summary>
        /// <param name="key">Ключ значения</param>
        /// <param name="value">Устанавливаемое значение</param>
        [ContextMethod("УстановитьСтроку")]
        public void SetString(string key, string value)
        {
            _requestSession.SetString(key, value);
        }

        /// <summary>
        /// Получить числовое значение из сессии
        /// </summary>
        /// <param name="key">Ключ значения</param>
        /// <returns></returns>
        [ContextMethod("ПолучитьЧисло")]
        public IValue GetNumber(string key)
        {
            var num = _requestSession.GetInt32(key);
            return num == null ? ValueFactory.Create() : ValueFactory.Create((decimal) num.Value);
        }

        /// <summary>
        /// Установить числовое значение в сессию
        /// </summary>
        /// <param name="key">Ключ значения</param>
        /// <param name="value">Устанавливаемое значение</param>
        [ContextMethod("УстановитьЧисло")]
        public void SetNumber(string key, int value)
        {
            _requestSession.SetInt32(key, value);
        }

        /// <summary>
        /// Очистить все значения сессии
        /// </summary>
        [ContextMethod("Очистить")]
        public void Clear()
        {
            _requestSession.Clear();
        }

        /// <summary>
        /// Удалить значение из сессии
        /// </summary>
        /// <param name="key">Ключ значения</param>
        [ContextMethod("Удалить")]
        public void Remove(string key)
        {
            _requestSession.Remove(key);
        }

        public object UnderlyingObject => _requestSession;
    }
}
