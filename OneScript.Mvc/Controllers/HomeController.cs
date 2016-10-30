using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace OneScript.Mvc.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {

            ViewBag.Title = "Page Title";
            StructureImpl model = (StructureImpl)StructureImpl.Constructor();
            model.Insert("DoModelPropertiesWork", ValueFactory.Create("Model properties work!"));
            model.Insert("DoesHtmlEncodingWork", ValueFactory.Create("<em>Should this be encoded?</em>"));
            model.Insert("DoesInternationalCharacterEncodingWork", ValueFactory.Create("Iñtërnâtiônàlizætiøn"));
            model.Insert("DoesRussianCharacterEncodingWork", ValueFactory.Create("Привет, как дела"));

            return View("Index","_Layout", model);
        }

    }
}
