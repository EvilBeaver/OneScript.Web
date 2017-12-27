using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    [ContextClass("РезультатДействияСтраница")]
    public class ViewActionResult : AutoContext<ViewActionResult>, IObjectWrapper
    {
        private ViewResult _result = new ViewResult();
        private ViewDataDictionaryWrapper _scriptViewData;

        public ViewActionResult()
        {
            _scriptViewData = new ViewDataDictionaryWrapper(_result.ViewData);
        }

        [ContextProperty("ИмяШаблона")]
        public string ViewName { get=>_result.ViewName; set=>_result.ViewName = value; }

        [ContextProperty("ТипСодержимого")]
        public string ContentType { get => _result.ContentType; set => _result.ContentType = value; }

        [ContextProperty("ТипСодержимого")]
        public int StatusCode { get => _result.StatusCode??200; set => _result.StatusCode = value; }

        [ContextProperty("ДанныеПредставления")]
        public ViewDataDictionaryWrapper ViewData
        {
            get => _scriptViewData;
            set
            {
                _scriptViewData = value;
                _result.ViewData = (ViewDataDictionary) _scriptViewData.UnderlyingObject;
            }
        }
        
        public object UnderlyingObject => _result;

        [ScriptConstructor]
        public static ViewActionResult Constructor()
        {
            return new ViewActionResult();
        }
    }

    [ContextClass("КоллекцияДанныхПредставления")]
    public class ViewDataDictionaryWrapper : ContextIValueImpl, IObjectWrapper
    {
        private readonly ViewDataDictionary _source;

        public ViewDataDictionaryWrapper(ViewDataDictionary source)
        {
            if(_source == null)
                _source = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

            _source = source;
        }

        public IValue this[string index]
        {
            get { return (IValue)_source[index]; }
            set { _source[index] = value; }
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index)
        {
            return (IValue)_source[index.AsString()];
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            _source[index.AsString()] = val.GetRawValue();
        }

        public object Model
        {
            get => _source.Model;
            set => _source.Model = value;
        }

        public object UnderlyingObject => _source;
    }
}
