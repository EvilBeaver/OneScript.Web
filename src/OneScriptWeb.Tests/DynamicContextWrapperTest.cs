﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using Xunit;
using System.Dynamic;
using ScriptEngine;

namespace OneScriptWeb.Tests
{
    public class DynamicContextWrapperTest
    {

        [Fact]
        public void WrapStructureProperties()
        {
            lock (TestOrderingLock.Lock)
            {
                var se = new MinimalTypeSystemHack();

                var structure = new StructureImpl("Свойство1,Свойство2",
                    ValueFactory.Create(1),
                    ValueFactory.Create("Hello"));

                dynamic dynStructure = new DynamicContextWrapper(structure);
                Assert.Equal<int>(1, (int)dynStructure.Свойство1);
                Assert.Equal<string>("Hello", dynStructure.Свойство2); 
            }
        }

        [Fact]
        public void WrapStructureIndices()
        {
            lock (TestOrderingLock.Lock)
            {
                var se = new MinimalTypeSystemHack();

                var structure = new StructureImpl("Свойство1,Свойство2",
                    ValueFactory.Create(1),
                    ValueFactory.Create("Hello"));

                dynamic dynStructure = new DynamicContextWrapper(structure);
                Assert.Equal<int>(1, (int)dynStructure["Свойство1"]);
                Assert.Equal<string>("Hello", dynStructure["Свойство2"]); 
            }
        }

        [Fact]
        public void WrapStructureMethodsCall()
        {
            lock (TestOrderingLock.Lock)
            {
                var se = new MinimalTypeSystemHack();

                var structure = new StructureImpl();
                dynamic dynStructure = new DynamicContextWrapper(structure);

                dynStructure.Вставить("Свойство1", 1);
                dynStructure.Вставить("Свойство2", "Hello");

                Assert.Equal<int>(1, (int)dynStructure["Свойство1"]);
                Assert.Equal<string>("Hello", dynStructure["Свойство2"]); 
            }
        }

        [Fact]
        public void WrapStructureEnumeration()
        {
            lock (TestOrderingLock.Lock)
            {
                var se = new MinimalTypeSystemHack();

                var structure = new StructureImpl();
                dynamic dynStructure = new DynamicContextWrapper(structure);

                dynStructure.Вставить("Свойство1", 1);
                dynStructure.Вставить("Свойство2", "Hello");

                int cnt = 0;
                foreach (var kv in dynStructure)
                {
                    ++cnt;
                    Assert.IsType(typeof(DynamicContextWrapper), kv);
                }

                Assert.Equal(2, cnt); 
            }
        }
    }
}
