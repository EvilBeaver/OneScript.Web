using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [NonViewComponent]
    public class ScriptedViewComponent : ScriptDrivenObject
    {
        public const string InvokeMethodNameRu = "ОбработкаВызова";
        public const string InvokeMethodNameEn = "CallProcessing";

        private Func<object> _invoker;

        public ScriptedViewComponent(LoadedModule module, string dynamicTypeName) : base(module, true)
        {
            var td = TypeManager.RegisterType(dynamicTypeName, typeof(ScriptedViewComponent));           
            DefineType(td);
            InitOwnData();


            var methId = GetScriptMethod(InvokeMethodNameRu, InvokeMethodNameEn);
            if (methId == -1)
            {
                throw new RuntimeException("Invoke method not found");
            }

            object Invoker()
            {
                CallAsFunction(methId, new IValue[0], out IValue result);
                return CustomMarshaller.ConvertToCLRObject(result);
            }

            _invoker = Invoker;
        }

        public IViewComponentResult Invoke()
        {
            return _invoker() as IViewComponentResult;
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
