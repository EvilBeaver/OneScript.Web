/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Application
{
    /// <summary>
    /// Р“Р»Р°РІРЅС‹Р№ РєР»Р°СЃСЃ, РѕС‚РІРµС‡Р°СЋС‰РёР№ Р·Р° РѕР±СЂР°Р±РѕС‚РєСѓ РІС…РѕРґСЏС‰РµРіРѕ Р·Р°РїСЂРѕСЃР° Рё РіРµРЅРµСЂР°С†РёСЋ РѕС‚РІРµС‚Р°.
    /// </summary>
    [ContextClass("РљРѕРЅС‚СЂРѕР»Р»РµСЂ")]
    [NonController]
    public class ScriptedController : AutoScriptDrivenObject<ScriptedController>
    {
        private ControllerContext _ctx;
        private IUrlHelper _url;

        private SessionImpl _session;

        private ViewDataDictionaryWrapper _osViewData;
        private ModelStateDictionaryWrapper _modelState;
        
        public ScriptedController(ControllerContext context, LoadedModule module) : base(module, true)
        {
            _ctx = context;
            HttpRequest = new HttpRequestImpl(_ctx.HttpContext.Request);
            HttpResponse = new HttpResponseImpl(_ctx.HttpContext.Response);
            
            if (_ctx.RouteData != null)
            {
                RouteValues = new MapImpl();
                foreach (var routeData in _ctx.RouteData.Values)
                {
                    var rv = RouteValues.AsObject();
                    rv.SetIndexedValue(
                        ValueFactory.Create(routeData.Key),
                        CustomMarshaller.ConvertToIValueSafe(routeData.Value, routeData.Value?.GetType())
                    );
                }
            }
            else
                RouteValues = ValueFactory.Create();

            var typeClr = (Type)context.ActionDescriptor.Properties["type"];
            var type = TypeManager.RegisterType("РљРѕРЅС‚СЂРѕР»Р»РµСЂ."+typeClr.Name, typeof(ScriptedController));
            DefineType(type);
            InitOwnData();
        }

        public void VoidAction(params object[] parameters)
        {
            var meth = (System.Reflection.MethodInfo)_ctx.ActionDescriptor.Properties["actionMethod"];
            if(parameters == null)
                parameters = new object[0];
            meth.Invoke(this, parameters);
        }

        public IActionResult ResultAction()
        {
            var meth = (System.Reflection.MethodInfo)_ctx.ActionDescriptor.Properties["actionMethod"];
            
            IActionResult result;
            if (meth is ReflectedMethodInfo reflected)
            {
                var res = reflected.InvokeDirect(this, new IValue[0]);

                result = res is IObjectWrapper wrapper
                    ? wrapper.UnderlyingObject as IActionResult
                    : res as IActionResult;
            }
            else
                result = meth.Invoke(this, new object[0]) as IActionResult;

            if(result == null)
                throw new InvalidOperationException("Function must return an IActionResult value");

            return result;
        }

        [ViewDataDictionary]
        public ViewDataDictionary FrameworkViewData
        {
            get { return ViewData?.GetDictionary();}
            set
            {
                ViewData = new ViewDataDictionaryWrapper(value);
            }
        }

        /// <summary>
        /// Р’С…РѕРґСЏС‰РёР№ Р·Р°РїСЂРѕСЃ HTTP
        /// </summary>
        [ContextProperty("Р—Р°РїСЂРѕСЃHttp")]
        public HttpRequestImpl HttpRequest { get; }

        /// <summary>
        /// РСЃС…РѕРґСЏС‰РёР№ РѕС‚РІРµС‚ HTTP
        /// </summary>
        [ContextProperty("РћС‚РІРµС‚Http")]
        public HttpResponseImpl HttpResponse { get; }

        /// <summary>
        /// Р”РµР№СЃС‚РІСѓСЋС‰РёРµ Р·РЅР°С‡РµРЅРёСЏ РјР°СЂС€СЂСѓС‚Р° РґР»СЏ С‚РµРєСѓС‰РµРіРѕ РІС‹Р·РѕРІР°.
        /// РўРёРї: РЎРѕРѕС‚РІРµС‚СЃС‚РІРёРµ РёР»Рё РќРµРѕРїСЂРµРґРµР»РµРЅРѕ.
        /// РљР»СЋС‡Р°РјРё СЃРѕРѕС‚РІРµС‚СЃС‚РІРёСЏ СЏРІР»СЏСЋС‚СЃСЏ РїРµСЂРµРјРµРЅРЅС‹Рµ РјР°СЂС€СЂСѓС‚Р°.
        /// </summary>
        [ContextProperty("Р—РЅР°С‡РµРЅРёСЏРњР°СЂС€СЂСѓС‚Р°")]
        public IValue RouteValues { get; }

        /// <summary>
        /// Р”Р°РЅРЅС‹Рµ http-СЃРµСЃСЃРёРё. РњРµС…Р°РЅРёР·Рј СЃРµСЃСЃРёР№ РёСЃРїРѕР»СЊР·СѓРµС‚ Cookies РґР»СЏ РїСЂРёРІСЏР·РєРё СЃРµСЃСЃРёРё Рё InMemory С…СЂР°РЅРёР»РёС‰Рµ РґР»СЏ РґР°РЅРЅС‹С… СЃРµСЃСЃРёРё.
        /// </summary>
        [ContextProperty("РЎРµСЃСЃРёСЏ")]
        public SessionImpl Session
        {
            get
            {
                if(_session == null)
                    _session = new SessionImpl(_ctx.HttpContext.Session);
                return _session;
            }
        }

        /// <summary>
        /// РЎРїРµС†РёР°Р»РёР·РёСЂРѕРІР°РЅРЅС‹Р№ РѕР±СЉРµРєС‚, РїСЂРµРґРЅР°Р·РЅР°С‡РµРЅРЅС‹Р№ РґР»СЏ РїРµСЂРµРґР°С‡Рё РґР°РЅРЅС‹С… РІ РіРµРЅРµСЂРёСЂСѓРµРјРѕРµ РџСЂРµРґСЃС‚Р°РІР»РµРЅРёРµ.
        /// Р­Р»РµРјРµРЅС‚С‹ РєРѕР»Р»РµРєС†РёРё РґРѕСЃС‚СѓРїРЅС‹ РІ РџСЂРµРґСЃС‚Р°РІР»РµРЅРёРё С‡РµСЂРµР· СЃРІРѕР№СЃС‚РІР° ViewBag Рё ViewData.
        /// </summary>
        [ContextProperty("Р”Р°РЅРЅС‹РµРџСЂРµРґСЃС‚Р°РІР»РµРЅРёСЏ")]
        public ViewDataDictionaryWrapper ViewData
        {
            get => _osViewData ?? (_osViewData = new ViewDataDictionaryWrapper());
            set => _osViewData = value ?? throw new ArgumentException();
        }

        [ContextProperty("РЎРѕСЃС‚РѕСЏРЅРёРµРњРѕРґРµР»Рё", "ModelState")]
        public ModelStateDictionaryWrapper ModelState =>
            _modelState ?? (_modelState = new ModelStateDictionaryWrapper(_ctx.ModelState));
        
        /// <summary>
        /// Р’СЃРїРѕРјРѕРіР°С‚РµР»СЊРЅС‹Р№ РјРµС‚РѕРґ РіРµРЅРµСЂР°С†РёРё РѕС‚РІРµС‚Р° РІ РІРёРґРµ РїСЂРµРґСЃС‚Р°РІР»РµРЅРёСЏ.
        /// </summary>
        /// <param name="nameOrModel">РРјСЏ РїСЂРµРґСЃС‚Р°РІР»РµРЅРёСЏ РёР»Рё РѕР±СЉРµРєС‚ РњРѕРґРµР»Рё (РµСЃР»Рё РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ РїСЂРµРґСЃС‚Р°РІР»РµРЅРёРµ РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ)</param>
        /// <param name="model">РћР±СЉРµРєС‚ РјРѕРґРµР»Рё (РїСЂРѕРёР·РІРѕР»СЊРЅС‹Р№)</param>
        /// <returns>Р РµР·СѓР»СЊС‚Р°С‚Р”РµР№СЃС‚РІРёСЏРџСЂРµРґСЃС‚Р°РІР»РµРЅРёРµ.</returns>
        [ContextMethod("РџСЂРµРґСЃС‚Р°РІР»РµРЅРёРµ")]
        public ViewActionResult View(IValue nameOrModel = null, IValue model = null)
        {
            if (nameOrModel == null && model == null)
            {
                return DefaultViewResult();
            }

            if (model == null)
            {
                if (nameOrModel.DataType == DataType.String)
                {
                    return ViewResultByName(nameOrModel.AsString(), null);
                }
                else
                {
                    return ViewResultByName(null, nameOrModel);
                }
            }

            if (nameOrModel == null)
                return ViewResultByName(null, model);

            return ViewResultByName(nameOrModel.AsString(), model);
        }

        /// <summary>
        /// Р’СЃРїРѕРјРѕРіР°С‚РµР»СЊРЅС‹Р№ РјРµС‚РѕРґ РіРµРЅРµСЂР°С†РёРё РѕС‚РІРµС‚Р° РІ РІРёРґРµ С‚РµРєСЃС‚РѕРІРѕРіРѕ СЃРѕРґРµСЂР¶РёРјРѕРіРѕ
        /// </summary>
        /// <param name="content">РЎРѕРґРµСЂР¶РёРјРѕРµ РѕС‚РІРµС‚Р°</param>
        /// <param name="contentType">РљРѕРґРёСЂРѕРІРєР° С‚РµРєСЃС‚Р° РѕС‚РІРµС‚Р°</param>
        /// <returns>Р РµР·СѓР»СЊС‚Р°С‚Р”РµР№СЃС‚РІРёСЏРЎРѕРґРµСЂР¶РёРјРѕРµ</returns>
        [ContextMethod("РЎРѕРґРµСЂР¶РёРјРѕРµ")]
        public ContentActionResult Content(string content, string contentType = null)
        {
            var ctResult = new ContentActionResult()
            {
                Content = content,
                ContentType = contentType
            };

            return ctResult;
        }

        /// <summary>
        /// Р’СЃРїРѕРјРѕРіР°С‚РµР»СЊРЅС‹Р№ РјРµС‚РѕРґ РіРµРЅРµСЂР°С†РёРё РѕС‚РІРµС‚Р° РІ РІРёРґРµ СЃРєР°С‡РёРІР°РµРјРѕРіРѕ С„Р°Р№Р»Р°.
        /// </summary>
        /// <param name="data">Р”Р°РЅРЅС‹Рµ С„Р°Р№Р»Р° (РїСѓС‚СЊ РёР»Рё Р”РІРѕРёС‡РЅС‹РµР”Р°РЅРЅС‹Рµ)</param>
        /// <param name="contentType">РЎРѕРґРµСЂР¶РёРјРѕРµ Р·Р°РіРѕР»РѕРІРєР° Content-type</param>
        /// <param name="downloadFileName">РРјСЏ СЃРєР°С‡РёРІР°РµРјРѕРіРѕ С„Р°Р№Р»Р°</param>
        /// <returns>Р РµР·СѓР»СЊС‚Р°С‚Р”РµР№СЃС‚РІРёСЏР¤Р°Р№Р»</returns>
        [ContextMethod("Р¤Р°Р№Р»")]
        public FileActionResult File(IValue data, string contentType = null, string downloadFileName = null)
        {
            FileActionResult fileResult;
            if (data.DataType == DataType.String)
            {
                fileResult = new FileActionResult(data.AsString(), contentType);
            }
            else
            {
                var obj = data.AsObject() as BinaryDataContext;
                if (obj == null)
                    throw RuntimeException.InvalidArgumentType(nameof(data));

                fileResult = new FileActionResult(obj, contentType);
            }

            if (downloadFileName != null)
                fileResult.DownloadFileName = downloadFileName;

            return fileResult;
        }

        /// <summary>
        /// Р’СЃРїРѕРјРѕРіР°С‚РµР»СЊРЅС‹Р№ РјРµС‚РѕРґ, РіРµРЅРµСЂРёСЂСѓСЋС‰РёР№ РєРѕРґ СЃРѕСЃС‚РѕСЏРЅРёСЏ HTTP
        /// </summary>
        /// <param name="code">РљРѕРґ СЃРѕСЃС‚РѕСЏРЅРёСЏ</param>
        /// <returns>Р РµР·СѓР»СЊС‚Р°С‚Р”РµР№СЃС‚РІРёСЏРљРѕРґРЎРѕСЃС‚РѕСЏРЅРёСЏ</returns>
        [ContextMethod("РљРѕРґРЎРѕСЃС‚РѕСЏРЅРёСЏ")]
        public StatusCodeActionResult StatusCode(int code)
        {
            return StatusCodeActionResult.Constructor(code);
        }

        /// <summary>
        /// Р’СЃРїРѕРјРѕРіР°С‚РµР»СЊРЅС‹Р№ РјРµС‚РѕРґ, РіРµРЅРµСЂРёСЂСѓСЋС‰РёР№ РѕС‚РІРµС‚ РІ РІРёРґРµ http-СЂРµРґРёСЂРµРєС‚Р°
        /// </summary>
        /// <param name="url">РђРґСЂРµСЃ РїРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёСЏ</param>
        /// <param name="permanent">РџСЂРёР·РЅР°Рє РїРѕСЃС‚РѕСЏРЅРЅРѕРіРѕ (permanent) РїРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёСЏ.</param>
        /// <returns>Р РµР·СѓР»СЊС‚Р°С‚Р”РµР№СЃС‚РІРёСЏРџРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёРµ</returns>
        [ContextMethod("РџРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёРµ")]
        public RedirectActionResult Redirect(string url, bool permanent = false)
        {
            return RedirectActionResult.Create(url, permanent);
        }

        /// <summary>
        /// Р’СЃРїРѕРјРѕРіР°С‚РµР»СЊРЅС‹Р№ РјРµС‚РѕРґ, РіРµРЅРµСЂРёСЂСѓСЋС‰РёР№ РѕС‚РІРµС‚ РІ РІРёРґРµ http-СЂРµРґРёСЂРµРєС‚Р°
        /// </summary>
        /// <param name="action">РРјСЏ РґРµР№СЃС‚РІРёСЏ РїРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёСЏ</param>
        /// <param name="controller">РљРѕРЅС‚СЂРѕР»Р»РµСЂ РїРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёСЏ</param>
        /// <param name="fields">Р”РѕРїРѕР»РЅРёС‚РµР»СЊРЅС‹Рµ РїРѕР»СЏ</param>
        /// <param name="permanent">РџСЂРёР·РЅР°Рє РїРѕСЃС‚РѕСЏРЅРЅРѕРіРѕ (permanent) РїРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёСЏ.</param>
        /// <returns>Р РµР·СѓР»СЊС‚Р°С‚Р”РµР№СЃС‚РІРёСЏРџРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёРµ</returns>
        [ContextMethod("РџРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёРµРќР°Р”РµР№СЃС‚РІРёРµ")]
        public RedirectActionResult RedirectToAction(string action, string controller = null, StructureImpl fields = null, bool permanent = false)
        {
            if(fields == null)
                fields = new StructureImpl();

            if(controller != null)
                fields.Insert("controller", ValueFactory.Create(controller));

            var url = ActionUrl(action, fields);
            if(url == null)
                throw new RuntimeException("РќРµ РѕР±РЅР°СЂСѓР¶РµРЅ Р·Р°РґР°РЅРЅС‹Р№ РјР°СЂС€СЂСѓС‚.");

            return RedirectActionResult.Create(url, permanent);
        }

        /// <summary>
        /// Р“РµРЅРµСЂРёСЂСѓРµС‚ URL РґР»СЏ РјР°СЂС€СЂСѓС‚Р°, Р·Р°РґР°РЅРЅРѕРіРѕ РІ РїСЂРёР»РѕР¶РµРЅРёРё.
        /// РџР°СЂР°РјРµС‚СЂ routeName РїРѕР·РІРѕР»СЏРµС‚ Р¶РµСЃС‚РєРѕ РїСЂРёРІСЏР·Р°С‚СЊ РіРµРЅРµСЂР°С†РёСЋ Р°РґСЂРµСЃР° Рє РєРѕРЅРєСЂРµС‚РЅРѕРјСѓ РјР°СЂС€СЂСѓС‚Сѓ
        /// </summary>
        /// <param name="routeName">РЎС‚СЂРѕРєР°. РРјСЏ РјР°СЂС€СЂСѓС‚Р°</param>
        /// <param name="fields">РЎС‚СЂСѓРєС‚СѓСЂР°. РџРѕР»СЏ РјР°СЂС€СЂСѓС‚Р° РІ РІРёРґРµ СЃС‚СЂСѓРєС‚СѓСЂС‹.</param>
        /// <returns>Р РµР·СѓР»СЊС‚Р°С‚Р”РµР№СЃС‚РІРёСЏРџРµСЂРµРЅР°РїСЂР°РІР»РµРЅРёРµ</returns>
        [ContextMethod("РђРґСЂРµСЃРњР°СЂС€СЂСѓС‚Р°")]
        public string RouteUrl(string routeName = null, StructureImpl fields = null)
        {
            string result;
            if (fields != null)
            {
                var values = new Dictionary<string, object>();
                foreach (var kv in fields)
                {
                    values.Add(kv.Key.AsString(), CustomMarshaller.ConvertToCLRObject(kv.Value));
                }

                result = routeName != null ? Url.RouteUrl(routeName, values) : Url.RouteUrl(values);
            }
            else
            {
                if (routeName == null)
                    throw RuntimeException.TooFewArgumentsPassed();

                result = Url.RouteUrl(routeName);
            }

            return result;
        }

        /// <summary>
        /// Р“РµРЅРµСЂРёСЂСѓРµС‚ Url РґР»СЏ РґРµР№СЃС‚РІРёСЏ РІ РєРѕРЅС‚СЂРѕР»Р»РµСЂРµ
        /// </summary>
        /// <param name="action">РРјСЏ РґРµР№СЃС‚РІРёСЏ</param>
        /// <param name="fieldsOrController">РРјСЏ РєРѕРЅС‚СЂРѕР»Р»РµСЂР° СЃС‚СЂРѕРєРѕР№ РёР»Рё СЃС‚СЂСѓРєС‚СѓСЂР°/СЃРѕРѕС‚РІРµС‚СЃС‚РІРёРµ РїРѕР»РµР№ РјР°СЂС€СЂСѓС‚Р°.</param>
        /// <returns></returns>
        [ContextMethod("РђРґСЂРµСЃР”РµР№СЃС‚РІРёСЏ", "ActionUrl")]
        public string ActionUrl(string action, IValue fieldsOrController = null)
        {
            string result;
            if (fieldsOrController != null)
            {
                if (fieldsOrController.DataType == DataType.String)
                {
                    result = Url.Action(action, fieldsOrController.AsString());
                }
                else if(fieldsOrController.GetRawValue() is IEnumerable<KeyAndValueImpl>)
                {
                    var values = new Dictionary<string, object>();
                    foreach (var kv in (IEnumerable<KeyAndValueImpl>) fieldsOrController.GetRawValue())
                    {
                        values.Add(kv.Key.AsString(), CustomMarshaller.ConvertToCLRObject(kv.Value));
                    }

                    result = Url.Action(action, values);
                }
                else
                {
                    throw RuntimeException.InvalidArgumentType(nameof(fieldsOrController));
                }
                
            }
            else
            {
                result = Url.Action(action);
            }

            return result;
        }
        
        public IUrlHelper Url
        {
            get
            {
                if (_url == null)
                {
                    var services = _ctx?.HttpContext?.RequestServices;
                    if (services == null)
                    {
                        return null;
                    }

                    var factory = services.GetRequiredService<IUrlHelperFactory>();
                    _url = factory.GetUrlHelper(_ctx);
                }

                return _url;
            }
        }

        private ViewActionResult ViewResultByName(string viewname, IValue model)
        {
            if(model != null)
                ViewData.Model = model;

            var va = new ViewActionResult()
            {
                ViewName = viewname,
                ViewData = ViewData
            };

            return va;
        }

        private ViewActionResult DefaultViewResult()
        {
            return new ViewActionResult() { ViewData = ViewData };
        }

        // TODO: РљРѕСЃС‚С‹Р»СЊ РІС‹Р·РІР°РЅРЅС‹Р№ РѕС€РёР±РєРѕР№ https://github.com/EvilBeaver/OneScript/issues/660
        internal static int GetOwnMethodsRelectionOffset()
        {
            return _ownMethods.Count;
        }
    }
}
