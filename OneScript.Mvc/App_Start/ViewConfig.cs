using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nustache.Mvc;

namespace OneScript.Mvc
{
    public class ViewConfig
    {
        public static void RegisterViewEngines(ViewEngineCollection engines)
        {
            engines.Insert(0, new NustacheViewEngine
            {
                // Comment out this line to require Model in front of all your expressions.
                // This makes it easier to share templates between the client and server.
                // But it also means that ViewData/ViewBag is inaccessible.
                RootContext = NustacheViewEngineRootContext.Model
            });
        }
    }
}