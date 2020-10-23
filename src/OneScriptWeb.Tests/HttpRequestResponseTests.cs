/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

            Assert.Equal("123456", testedHeaders["Content-Length"]);
            Assert.Equal("utf-8", testedHeaders["Content-Encoding"]); 
        }

        [Fact]
        public void ContentTypeIsReflectedInHeadersAfterAssignment()
        {
            var response = MockRequestClass();

            var scriptRequest = new HttpResponseImpl(response);
            scriptRequest.ContentType = "text/plain";
            Assert.Equal("text/plain", scriptRequest.Headers.GetIndexedValue(ValueFactory.Create("Content-Type")).AsString());
            Assert.Equal("text/plain", scriptRequest.RealObject.Headers["Content-Type"]);
            Assert.Equal("text/plain", scriptRequest.RealObject.ContentType); 
        }

        [Fact]
        public void ContentTypeIsReflectedInHeadersAfterSetHeaders()
        {
            var response = MockRequestClass();

            var headers = new MapImpl();
            headers.SetIndexedValue(ValueFactory.Create("Content-Type"), ValueFactory.Create("text/plain"));

            var scriptRequest = new HttpResponseImpl(response);
            scriptRequest.SetHeaders(headers);
            Assert.Equal("text/plain", scriptRequest.Headers.GetIndexedValue(ValueFactory.Create("Content-Type")).AsString());
            Assert.Equal("text/plain", scriptRequest.RealObject.Headers["Content-Type"]);
            Assert.Equal("text/plain", scriptRequest.RealObject.ContentType); 
        }
        
        [Fact]
        public void FormDataIsAccessible()
        {
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
            Assert.IsType<FormDataCollectionContext>(request.FormData);
            Assert.IsType<FormFilesCollectionContext>(request.FormData.Files);

            var fFile = request.FormData.Files[0];
            var fFileInt = request.FormData.Files["uploaded"];

            Assert.Equal(fFile, fFileInt);
        }

        [Fact]
        public void CookiesAreAccessible()
        {
            var fakeCookies = new Dictionary<string,string>();
            fakeCookies["test"] = "test";
            var reqCookies = new Mock<IRequestCookieCollection>();
            reqCookies.Setup(x => x.GetEnumerator()).Returns(fakeCookies.GetEnumerator());
            var requestMock = new Mock<HttpRequest>();
            requestMock.SetupGet(x => x.Cookies).Returns(reqCookies.Object);

            var request = new HttpRequestImpl(requestMock.Object);

            Assert.Equal("test", request.Cookies.GetIndexedValue(ValueFactory.Create("test")).AsString());
        }

        private HttpResponse MockRequestClass()
        {
            var mock = new Mock<HttpResponse>();

            var dict = new HeaderDictionary();
            mock.SetupGet(x => x.Headers).Returns(dict);
            mock.SetupGet(x => x.ContentType).Returns(() => dict["Content-Type"].ToString());
            mock.SetupSet(x => x.ContentType).Callback(x => dict["Content-Type"] = x);
            
            return mock.Object;
        }
    }
}
