/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
            var osResult = new ViewActionResult();
            osResult.ViewData = InitViewData();
            osResult.ViewData["MyData"] = ValueFactory.Create("string data");

            Assert.Equal("string data", osResult.ViewData.GetIndexedValue(ValueFactory.Create("MyData")).AsString());    
        }

        private ViewDataDictionaryWrapper InitViewData()
        {
            return new ViewDataDictionaryWrapper();
        }

        [Fact]
        public void CanGetViewResult()
        {
            var osResult = new ViewActionResult();
                
            osResult.ViewData = InitViewData();
            osResult.ViewData["MyData"] = ValueFactory.Create("string data");
            osResult.ViewData["MyObject"] = new StructureImpl();

            osResult.ViewData.Model = new ArrayImpl();

            var realAction = osResult.CreateExecutableResult();
            Assert.IsType<string>(realAction.ViewData["MyData"]);
            Assert.IsType<DynamicContextWrapper>(realAction.ViewData["MyObject"]);
            Assert.IsType<DynamicContextWrapper>(realAction.ViewData.Model);

            var structWrap = realAction.ViewData["MyObject"] as DynamicContextWrapper;
            var arrayWrap = realAction.ViewData.Model as DynamicContextWrapper;

            Assert.Equal(osResult.ViewData["MyObject"], structWrap.UnderlyingObject);
            Assert.Equal(osResult.ViewData.Model, structWrap.UnderlyingObject);
        }
    }
}
