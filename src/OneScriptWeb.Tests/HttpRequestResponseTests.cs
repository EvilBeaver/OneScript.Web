using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;
using Moq;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace OneScriptWeb.Tests
{
    public class HttpRequestResponseTests
    {
        // инициализирует систему типов
        private static WebApplicationEngine wa = new WebApplicationEngine();

        [Fact]
        public void RequestHeadersAccessibleThrough_MapImpl()
        {
            var request = new Mock<HttpRequest>();
            var headers = new HeaderDictionary();
            headers.Add("Content-Length", "12312");
            headers.Add("Content-Encoding", "utf-8");
            request.SetupGet(x => x.Headers).Returns(headers);
            
            var scriptRequest = new HttpRequestImpl(request.Object);

            Assert.Equal(2, scriptRequest.Headers.Count());
            Assert.Equal(headers["Content-Length"], scriptRequest.Headers.GetIndexedValue(ValueFactory.Create("Content-Length")).AsString());
            Assert.Equal(headers["Content-Encoding"], scriptRequest.Headers.GetIndexedValue(ValueFactory.Create("Content-Encoding")).AsString());
        }

        [Fact]
        public void ResponseHeadersCanBeSetFrom_MapImpl()
        {
            var response = new Mock<HttpResponse>();
            var headers = new MapImpl();
            headers.SetIndexedValue(ValueFactory.Create("Content-Length"), ValueFactory.Create("123456"));
            headers.SetIndexedValue(ValueFactory.Create("Content-Encoding"), ValueFactory.Create("utf-8"));

            var testedHeaders = new HeaderDictionary();
            response.SetupGet(x => x.Headers).Returns(testedHeaders);

            var scriptRequest = new HttpResponseImpl(response.Object);
            scriptRequest.SetHeaders(headers);

            Assert.Equal(testedHeaders["Content-Length"],"123456");
            Assert.Equal(testedHeaders["Content-Encoding"],"utf-8");
        }

        [Fact]
        public void ContentTypeIsReflectedInHeadersAfterAssignment()
        {
            var context = new DefaultHttpContext();
            var response = new DefaultHttpResponse(context);
            
            var scriptRequest = new HttpResponseImpl(response);
            scriptRequest.ContentType = "text/plain";
            Assert.True(scriptRequest.Headers.GetIndexedValue(ValueFactory.Create("Content-Type")).AsString().Equals("text/plain"));
            Assert.Equal("text/plain", scriptRequest.RealObject.Headers["Content-Type"]);
            Assert.Equal("text/plain", scriptRequest.RealObject.ContentType);
        }

        [Fact]
        public void ContentTypeIsReflectedInHeadersAfterSetHeaders()
        {
            var context = new DefaultHttpContext();
            var response = new DefaultHttpResponse(context);

            var headers = new MapImpl();
            headers.SetIndexedValue(ValueFactory.Create("Content-Type"), ValueFactory.Create("text/plain"));

            var scriptRequest = new HttpResponseImpl(response);
            scriptRequest.SetHeaders(headers);
            Assert.True(scriptRequest.Headers.GetIndexedValue(ValueFactory.Create("Content-Type")).AsString().Equals("text/plain"));
            Assert.Equal("text/plain", scriptRequest.RealObject.Headers["Content-Type"]);
            Assert.Equal("text/plain", scriptRequest.RealObject.ContentType);
        }
    }
}
