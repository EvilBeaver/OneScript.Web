/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using OneScript.WebHost.Infrastructure;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.WebHost.Authorization
{
    public class AuthorizationModule : ScriptDrivenObject
    {
        private static ContextMethodsMapper<AuthorizationModule> _ownMethods = new ContextMethodsMapper<AuthorizationModule>();

        private readonly IFileProvider _filesystem;
        private readonly IApplicationRuntime _runtime;

        public AuthorizationModule(LoadedModule module, 
            IFileProvider filesystem,
            IApplicationRuntime runtime):base(module)
        {
            _filesystem = filesystem;
            _runtime = runtime;
        }

        protected override int GetOwnVariableCount()
        {
            return 0;
        }

        protected override int GetOwnMethodCount()
        {
            return _ownMethods.Count;
        }

        protected override void UpdateState()
        {
        }

        protected override int FindOwnMethod(string name)
        {
            try
            {
                int idx = _ownMethods.FindMethod(name);
                return idx;
            }
            catch (RuntimeException)
            {
                return -1;
            }
        }

        protected override MethodInfo GetOwnMethod(int index)
        {
            return _ownMethods.GetMethodInfo(index);
        }

        protected override IValue CallOwnFunction(int index, IValue[] arguments)
        {
            return _ownMethods.GetMethod(index)(this, arguments);
        }

        protected override void CallOwnProcedure(int index, IValue[] arguments)
        {
            _ownMethods.GetMethod(index)(this, arguments);
        }

        public void OnRegistration(ICollection<IAuthorizationHandler> handlers)
        {
            var methId = GetScriptMethod("ПриРегистрацииОбработчиков", "OnHandlersRegistration");
            if (methId == -1)
                return;

            ArrayImpl newClasses = new ArrayImpl();
            var args = new IValue[] {newClasses};

            CallScriptMethod(methId, args);

            foreach (var classPath in newClasses.Select(x=>x.AsString()))
            {
                var fInfo = _filesystem.GetFileInfo(classPath);
                if (!fInfo.Exists || fInfo.IsDirectory)
                {
                    throw new InvalidOperationException($"Module {classPath} is not found");
                }

                var codeSource = new FileInfoCodeSource(fInfo);
                var instance = ScriptedAuthorizationHandler.CreateInstance(codeSource, _runtime);

                handlers.Add(instance);
            }
        }

        public static AuthorizationModule CreateInstance(ICodeSource codeSource, IApplicationRuntime runtime, IFileProvider filesystem)
        {
            var compiler = runtime.Engine.GetCompilerService();

            for (int i = 0; i < _ownMethods.Count; i++)
            {
                compiler.DefineMethod(_ownMethods.GetMethodInfo(i));
            }

            var moduleImage = compiler.Compile(codeSource);
            var module = runtime.Engine.LoadModuleImage(moduleImage);
            
            return new AuthorizationModule(module, filesystem, runtime);
        }
    }
}
