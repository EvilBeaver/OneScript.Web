/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

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
