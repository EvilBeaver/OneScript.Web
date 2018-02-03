﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.Machine;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class SessionsTest
    {
        [Fact]
        public void SessionValues_CanBeSet()
        {
            lock (TestOrderingLock.Lock)
            {
                var wa = new WebApplicationEngine();

                var checks = new Dictionary<string, byte[]>();
                var session = CreateSessionMock(checks);

                var oscriptSession = new SessionImpl(session);
                oscriptSession.SetString("user", "EvilBeaver");

                Assert.Equal("EvilBeaver", Encoding.Default.GetString(checks["user"]));
            }
        }

        [Fact]
        public void SessionValues_CanBeRead()
        {
            lock (TestOrderingLock.Lock)
            {
                var wa = new WebApplicationEngine();

                var checks = new Dictionary<string, byte[]>();
                var session = CreateSessionMock(checks);

                var oscriptSession = new SessionImpl(session);
                oscriptSession.SetString("user", "EvilBeaver");
                oscriptSession.SetNumber("num", 1);
                
                Assert.Equal("EvilBeaver", oscriptSession.GetString("user").AsString());
                Assert.Equal(1m, oscriptSession.GetNumber("num").AsNumber());
            }
        }

        [Fact]
        public void SessionValues_CanGetKeys()
        {
            lock (TestOrderingLock.Lock)
            {
                var wa = new WebApplicationEngine();

                var checks = new Dictionary<string, byte[]>();
                var session = CreateSessionMock(checks);

                var oscriptSession = new SessionImpl(session);
                oscriptSession.SetString("user", "EvilBeaver");
                oscriptSession.SetString("password", "1");

                var arr = oscriptSession.GetKeys();
                Assert.Equal(2, arr.Count());
                Assert.NotEqual(arr.Find(ValueFactory.Create("user")), ValueFactory.Create());
                Assert.NotEqual(arr.Find(ValueFactory.Create("password")), ValueFactory.Create());
            }
        }

        private static ISession CreateSessionMock(Dictionary<string, byte[]> checks)
        {
            return new SessionMock(checks);
        }

        private class SessionMock : ISession
        {
            public SessionMock()
            {
                _values = new Dictionary<string, byte[]>();
            }

            public SessionMock(Dictionary<string, byte[]> vals)
            {
                _values = vals;
            }

            private Dictionary<string, byte[]> _values;

            public Task LoadAsync()
            {
                throw new NotImplementedException();
            }

            public Task CommitAsync()
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(string key, out byte[] value)
            {
                return _values.TryGetValue(key, out value);
            }

            public void Set(string key, byte[] value)
            {
                _values[key] = value;
            }

            public void Remove(string key)
            {
                _values.Remove(key);
            }

            public void Clear()
            {
                _values.Clear();
            }

            public bool IsAvailable { get; }
            public string Id { get; }
            public IEnumerable<string> Keys => _values.Keys;
        }
    }
}
