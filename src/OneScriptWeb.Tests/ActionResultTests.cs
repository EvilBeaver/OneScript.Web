using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OneScript.WebHost.Application;
using OneScript.WebHost.Infrastructure;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScriptWeb.Tests
{
    public class ActionResultTests
    {
        [Fact]
        public void ViewResultDataAccessibleThroughScript()
        {
            lock (TestOrderingLock.Lock)
            {
                var se = new MinimalTypeSystemHack();
                var osResult = new ViewActionResult();
                osResult.ViewData = InitViewData();
                osResult.ViewData["MyData"] = ValueFactory.Create("string data");

                Assert.Equal("string data", osResult.ViewData.GetIndexedValue(ValueFactory.Create("MyData")).AsString()); 
            }
        }

        private ViewDataDictionaryWrapper InitViewData()
        {
            return new ViewDataDictionaryWrapper();
        }

        [Fact]
        public void CanGetViewResult()
        {
            lock (TestOrderingLock.Lock)
            {
                var se = new MinimalTypeSystemHack();
                var osResult = new ViewActionResult();
                
                osResult.ViewData = InitViewData();
                osResult.ViewData["MyData"] = ValueFactory.Create("string data");
                osResult.ViewData["MyObject"] = new StructureImpl();

                osResult.ViewData.Model = new ArrayImpl();

                var realAction = osResult.CreateExecutableResult();
                Assert.IsType(typeof(string), realAction.ViewData["MyData"]);
                Assert.IsType(typeof(DynamicContextWrapper), realAction.ViewData["MyObject"]);
                Assert.IsType(typeof(DynamicContextWrapper), realAction.ViewData.Model);

                var structWrap = realAction.ViewData["MyObject"] as DynamicContextWrapper;
                var arrayWrap = realAction.ViewData.Model as DynamicContextWrapper;

                Assert.Equal(osResult.ViewData["MyObject"], structWrap.UnderlyingObject);
                Assert.Equal(osResult.ViewData.Model, structWrap.UnderlyingObject);

            }
        }
    }
}
