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
        [Fact]
        public void RequestHeadersAccessibleThrough_MapImpl()
        {
            lock (TestOrderingLock.Lock)
            {
                WebApplicationEngine wa = new WebApplicationEngine();
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
        }

        [Fact]
        public void ResponseHeadersCanBeSetFrom_MapImpl()
        {
            lock (TestOrderingLock.Lock)
            {
                WebApplicationEngine wa = new WebApplicationEngine();
                var response = new Mock<HttpResponse>();
                var headers = new MapImpl();
                headers.SetIndexedValue(ValueFactory.Create("Content-Length"), ValueFactory.Create("123456"));
                headers.SetIndexedValue(ValueFactory.Create("Content-Encoding"), ValueFactory.Create("utf-8"));

                var testedHeaders = new HeaderDictionary();
                response.SetupGet(x => x.Headers).Returns(testedHeaders);

                var scriptRequest = new HttpResponseImpl(response.Object);
                scriptRequest.SetHeaders(headers);

                Assert.Equal(testedHeaders["Content-Length"], "123456");
                Assert.Equal(testedHeaders["Content-Encoding"], "utf-8"); 
            }
        }

        [Fact]
        public void ContentTypeIsReflectedInHeadersAfterAssignment()
        {
            lock (TestOrderingLock.Lock)
            {
                WebApplicationEngine wa = new WebApplicationEngine();
                var context = new DefaultHttpContext();
                var response = new DefaultHttpResponse(context);

                var scriptRequest = new HttpResponseImpl(response);
                scriptRequest.ContentType = "text/plain";
                Assert.True(scriptRequest.Headers.GetIndexedValue(ValueFactory.Create("Content-Type")).AsString().Equals("text/plain"));
                Assert.Equal("text/plain", scriptRequest.RealObject.Headers["Content-Type"]);
                Assert.Equal("text/plain", scriptRequest.RealObject.ContentType); 
            }
        }

        [Fact]
        public void ContentTypeIsReflectedInHeadersAfterSetHeaders()
        {
            lock (TestOrderingLock.Lock)
            {
                WebApplicationEngine wa = new WebApplicationEngine();
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

        [Fact]
        public void FormDataIsAccessible()
        {
            lock (TestOrderingLock.Lock)
            {
                WebApplicationEngine wa = new WebApplicationEngine();

                var fileMock = new Mock<IFormFile>();
                fileMock.SetupGet(x => x.Name).Returns("uploaded");

                var filesMock = new Mock<IFormFileCollection>();
                filesMock.SetupGet(x => x.Count).Returns(1);
                filesMock.Setup(x => x.GetFile("uploaded")).Returns(fileMock.Object);
                filesMock.Setup(x => x.GetEnumerator()).Returns(() =>
                {
                    var arr = new List<IFormFile>
                    {
                        fileMock.Object
                    };
                    return arr.GetEnumerator();
                });

                var formMock = new Mock<IFormCollection>();
                formMock.SetupGet(x => x.Files).Returns(filesMock.Object);

                var requestMock = new Mock<HttpRequest>();
                requestMock.SetupGet(x => x.Form).Returns(formMock.Object);
                requestMock.SetupGet(x => x.Headers).Returns(new HeaderDictionary());

                var request = new HttpRequestImpl(requestMock.Object);

                Assert.Equal(1, request.FormData.Files.Count());
                Assert.IsType(typeof(FormDataCollectionContext), request.FormData);
                Assert.IsType(typeof(FormFilesCollectionContext), request.FormData.Files);

                var fFile = request.FormData.Files[0];
                var fFileInt = request.FormData.Files["uploaded"];

                Assert.Equal(fFile, fFileInt);

            }
        }
    }
}
