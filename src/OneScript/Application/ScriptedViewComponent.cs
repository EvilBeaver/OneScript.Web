using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [NonViewComponent]
    public class ScriptedViewComponent : ScriptDrivenObject
    {
        public ScriptedViewComponent(LoadedModule module, string dynamicTypeName) : base(module, true)
        {
            var td = TypeManager.RegisterType(dynamicTypeName, typeof(ScriptedViewComponent));           
            DefineType(td);
            InitOwnData();
        }

        [ViewComponentContext]
        public ViewComponentContext ComponentContext { get; set; }

        protected override int GetOwnVariableCount()
        {
            throw new NotImplementedException();
        }

        protected override int GetOwnMethodCount()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateState()
        {
            throw new NotImplementedException();
        }
    }
}
