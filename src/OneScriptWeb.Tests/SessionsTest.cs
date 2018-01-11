using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using OneScript.WebHost.Infrastructure;
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

                var checks = new Dictionary<string,string>();
                var session = CreateSessionMock(checks);

                var oscriptSession = new SessionImpl(session);
                oscriptSession.SetString("user", "EvilBeaver");

                Assert.Equal("EvilBeaver", checks["user"]);
            }
        }

        private static ISession CreateSessionMock(Dictionary<string, string> checks)
        {
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>())).Callback(new Action<string, byte[]>((k, v) =>
            {
                checks.Add(k, Encoding.Default.GetString(v));
            }));

            return sessionMock.Object;
        }
    }
}
