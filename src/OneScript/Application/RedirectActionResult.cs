﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("РезультатДействияПеренаправление")]
    public class RedirectActionResult : AutoContext<RedirectActionResult>, IObjectWrapper
    {
        private readonly RedirectResult _result;

        public RedirectActionResult(string url, bool permanent)
        {
            _result = new RedirectResult(url, permanent);
        }

        public object UnderlyingObject => _result;


        [ScriptConstructor]
        public static RedirectActionResult Create(string url, bool permanent = false)
        {
            return new RedirectActionResult(url, permanent);
        }
    }
}
