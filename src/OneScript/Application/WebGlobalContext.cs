/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using MethodInfo = ScriptEngine.Machine.MethodInfo;

namespace OneScript.WebHost.Application
{
    [GlobalContext(Category = "Глобальный контекст", ManualRegistration = true)]
    public class WebGlobalContext : IAttachableContext
    {
        // в вебе не все части SystemGlobalContext должны быть доступны
        // нужно разделить его внутри движка на консольную и общую части
        // а пока, спрячем его здесь и выставим только некоторые вещи
        private readonly RCIRedirector _osGlobal;

        public WebGlobalContext(IHostApplication host, ICodeSource entryScript) : this(host,entryScript,null)
        {   
        }

        public WebGlobalContext(IHostApplication host, ICodeSource entryScript, IApplicationRuntime webEng)
        {
            var sys = new SystemGlobalContext();
            sys.ApplicationHost = host;
            sys.CodeSource = entryScript;
            if (webEng != null)
                sys.EngineInstance = webEng.Engine;

            _osGlobal = new RCIRedirector(sys);
            _osGlobal.PublishProperty("Символы", null);
            _osGlobal.PublishProperty("Chars", null);
            _osGlobal.PublishProperty("ФайловыеПотоки", null);
            _osGlobal.PublishProperty("FileStreams", null);

            _osGlobal.PublishMethod("ОсвободитьОбъект", "FreeObject");
            _osGlobal.PublishMethod("ВыполнитьСборкуМусора", "RunGarbageCollection");
            _osGlobal.PublishMethod("ЗапуститьПриложение", "RunApp");
            _osGlobal.PublishMethod("СоздатьПроцесс", "CreateProcess");
            _osGlobal.PublishMethod("НайтиПроцессПоИдентификатору", "FindProcessById");
            _osGlobal.PublishMethod("НайтиПроцессыПоИмени", "FindProcessesByName");
            _osGlobal.PublishMethod("КраткоеПредставлениеОшибки", "BriefErrorDescription");
            _osGlobal.PublishMethod("КаталогПрограммы", "ProgramDirectory");
            _osGlobal.PublishMethod("ПодробноеПредставлениеОшибки", "DetailErrorDescription");
            _osGlobal.PublishMethod("ТекущаяУниверсальнаяДата", "CurrentUniversalDate");
            _osGlobal.PublishMethod("ТекущаяУниверсальнаяДатаВМиллисекундах", "CurrentUniversalDateInMilliseconds");
            _osGlobal.PublishMethod("ЗначениеЗаполнено", "IsValueFilled");
            _osGlobal.PublishMethod("ЗаполнитьЗначенияСвойств", "FillPropertyValues");
            _osGlobal.PublishMethod("ПолучитьCOMОбъект", "GetCOMObject");
            _osGlobal.PublishMethod("Приостановить", "Sleep");
            _osGlobal.PublishMethod("ПодключитьВнешнююКомпоненту", "AttachAddIn");
            _osGlobal.PublishMethod("ЗагрузитьСценарий", "LoadScript");
            _osGlobal.PublishMethod("ЗагрузитьСценарийИзСтроки", "LoadScriptFromString");
            _osGlobal.PublishMethod("ПодключитьСценарий", "AttachScript");
            _osGlobal.PublishMethod("Сообщить", "Message");
            _osGlobal.PublishMethod("СтартовыйСценарий", "StartupScript");

            sys.InitInstance();
        }

        public IValue GetIndexedValue(IValue index)
        {
            return _osGlobal.GetIndexedValue(index);
        }

        public void SetIndexedValue(IValue index, IValue val)
        {
            _osGlobal.SetIndexedValue(index, val);
        }

        public int FindProperty(string name)
        {
            return _osGlobal.FindProperty(name);
        }

        public bool IsPropReadable(int propNum)
        {
            return _osGlobal.IsPropReadable(propNum);
        }

        public bool IsPropWritable(int propNum)
        {
            return _osGlobal.IsPropWritable(propNum);
        }

        public IValue GetPropValue(int propNum)
        {
            return _osGlobal.GetPropValue(propNum);
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            _osGlobal.SetPropValue(propNum, newVal);
        }

        public int GetPropCount()
        {
            return _osGlobal.GetPropCount();
        }

        public string GetPropName(int propNum)
        {
            return _osGlobal.GetPropName(propNum);
        }

        public int FindMethod(string name)
        {
            return _osGlobal.FindMethod(name);
        }

        public int GetMethodsCount()
        {
            return _osGlobal.GetMethodsCount();
        }

        public MethodInfo GetMethodInfo(int methodNumber)
        {
            return _osGlobal.GetMethodInfo(methodNumber);
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            _osGlobal.CallAsProcedure(methodNumber, arguments);
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            _osGlobal.CallAsFunction(methodNumber, arguments, out retValue);
        }

        public void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodInfo[] methods)
        {
            var state = _osGlobal.GetProperties().ToArray();
            variables = new IVariable[state.Length];
            for (int i = 0; i < state.Length; i++)
            {
                variables[i] = Variable.CreateContextPropertyReference(_osGlobal, i, state[i].Identifier);
            }

            methods = _osGlobal.GetMethods().ToArray();
        }

        public bool IsIndexed
        {
            get { return _osGlobal.IsIndexed; }
        }

        public bool DynamicMethodSignatures
        {
            get { return _osGlobal.DynamicMethodSignatures; }
        }
    }

    // копия из ScriptEngine. Там она private, после переделки в public тут можно будет убрать
    public class IndexedNamesCollection
    {
        private readonly List<string> _names = new List<string>();
        private readonly Dictionary<string, int> _nameIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public bool TryGetIdOfName(string name, out int id)
        {
            return _nameIndexes.TryGetValue(name, out id);
        }

        public string GetName(int id)
        {
            return _names[id];
        }

        public int RegisterName(string name, string alias = null)
        {
            System.Diagnostics.Debug.Assert(name != null);

            int id = _names.Count;
            _nameIndexes.Add(name, id);
            _names.Add(name);
            if (alias != null)
                _nameIndexes.Add(alias, id);

            return id;
        }
    }


    class RCIRedirector : IRuntimeContextInstance
    {
        private readonly IRuntimeContextInstance _inst;
        private readonly Dictionary<int, int> _propRedirects = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _methRedirects = new Dictionary<int, int>();
        private readonly IndexedNamesCollection _thisPropIndexes = new IndexedNamesCollection();
        private readonly IndexedNamesCollection _thisMethIndexes = new IndexedNamesCollection();

        public RCIRedirector(IRuntimeContextInstance instance)
        {
            _inst = instance;
        }

        public void PublishProperty(string name, string alias)
        {
            var thisIndex = _thisPropIndexes.RegisterName(name, alias);
            var thatIndex = _inst.FindProperty(name);
            Debug.Assert(thatIndex >= 0);

            _propRedirects.Add(thisIndex, thatIndex);
        }

        public void PublishMethod(string name, string alias)
        {
            var thisIndex = _thisMethIndexes.RegisterName(name, alias);
            var thatIndex = _inst.FindMethod(name);
            Debug.Assert(thatIndex >= 0);

            _methRedirects.Add(thisIndex, thatIndex);
        }

        public IRuntimeContextInstance Instance => _inst;

        public IValue GetIndexedValue(IValue index)
        {
            return _inst.GetIndexedValue(index);
        }

        public void SetIndexedValue(IValue index, IValue val)
        {
            _inst.SetIndexedValue(index, val);
        }

        public int FindProperty(string name)
        {
            int id;
            if (!_thisPropIndexes.TryGetIdOfName(name, out id))
            {
                throw RuntimeException.PropNotFoundException(name);
            }

            return id;
        }

        public bool IsPropReadable(int propNum)
        {
            return _inst.IsPropReadable(_propRedirects[propNum]);
        }

        public bool IsPropWritable(int propNum)
        {
            return _inst.IsPropWritable(_propRedirects[propNum]);
        }

        public IValue GetPropValue(int propNum)
        {
            return _inst.GetPropValue(_propRedirects[propNum]);
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            _inst.SetPropValue(_propRedirects[propNum], newVal);
        }

        public int GetPropCount()
        {
            return _propRedirects.Count;
        }

        public string GetPropName(int propNum)
        {
            return _thisPropIndexes.GetName(propNum);
        }

        public int FindMethod(string name)
        {
            int id;
            if (!_thisMethIndexes.TryGetIdOfName(name, out id))
            {
                throw RuntimeException.MethodNotFoundException(name);
            }

            return id;
        }

        public int GetMethodsCount()
        {
            return _methRedirects.Count;
        }

        public MethodInfo GetMethodInfo(int methodNumber)
        {
            return _inst.GetMethodInfo(_methRedirects[methodNumber]);
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            _inst.CallAsProcedure(_methRedirects[methodNumber], arguments);
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            _inst.CallAsFunction(_methRedirects[methodNumber], arguments, out retValue);
        }

        public bool IsIndexed
        {
            get { return _inst.IsIndexed; }
        }

        public bool DynamicMethodSignatures
        {
            get { return _inst.DynamicMethodSignatures; }
        }
    }
}
