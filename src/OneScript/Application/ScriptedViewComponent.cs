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
        public const string InvokeMethodNameRu = "ОбработкаВызова";
        public const string InvokeMethodNameEn = "CallProcessing";

        public ScriptedViewComponent(LoadedModule module, string dynamicTypeName) : base(module, true)
        {
            var td = TypeManager.RegisterType(dynamicTypeName, typeof(ScriptedViewComponent));           
            DefineType(td);
            InitOwnData();
        }

        public ViewComponentResult Invoke()
        {
            var methId = GetScriptMethod(InvokeMethodNameRu, InvokeMethodNameEn);
            if (methId == -1)
            {
                return null;
            }

            CallAsFunction(methId, new IValue[0], out IValue result);

            return result.AsObject() as ViewComponentResult;
        }

        [ViewComponentContext]
        public ViewComponentContext ComponentContext { get; set; }

        protected override int GetOwnVariableCount()
        {
            return 0;
        }

        protected override int GetOwnMethodCount()
        {
            return 0;
        }

        protected override void UpdateState()
        {
            
        }
    }
}
