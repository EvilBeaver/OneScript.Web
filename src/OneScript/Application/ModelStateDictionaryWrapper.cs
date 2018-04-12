using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("СостояниеМодели", "ModelState")]
    public class ModelStateDictionaryWrapper : AutoContext<ModelStateDictionaryWrapper>, IObjectWrapper
    {
        private ModelStateDictionary _state;

        public ModelStateDictionaryWrapper(ModelStateDictionary source)
        {
            _state = source;
        }

        [ContextMethod("ДобавитьОшибку", "AddError")]
        public void AddError(string name, string errorText)
        {
            _state.AddModelError(name, errorText);
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _state.Clear();
        }

        [ContextProperty("Корректно")] public bool IsValid => _state.IsValid;

        public object UnderlyingObject
        {
            get { return _state; }
        }
    }
}
