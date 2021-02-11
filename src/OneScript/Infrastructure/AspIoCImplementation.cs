// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScriptEngine.Hosting;

namespace OneScript.WebHost.Infrastructure
{
    public class AspIoCImplementation : IServiceDefinitions, IServiceContainer
    {
        private readonly IServiceProvider _provider;
        private readonly IServiceCollection _services;
        
        public AspIoCImplementation(IServiceCollection services)
        {
            _services = services;
        }

        public AspIoCImplementation(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IServiceContainer CreateContainer()
        {
            throw new NotSupportedException();
        }

        public void Register(Type knownType)
        {
            _services.AddTransient(knownType);
        }

        public void Register(Type interfaceType, Type implementation)
        {
            _services.AddTransient(interfaceType, implementation);
        }

        public void Register<T>() where T : class
        {
            _services.AddTransient<T>();
        }

        public void Register<T>(T instance) where T : class
        {
            _services.AddSingleton(instance);
        }

        public void Register<T, TImpl>() where T : class where TImpl : class, T
        {
            _services.AddTransient<T, TImpl>();
        }

        public void Register<T>(Func<IServiceContainer, T> factory) where T : class
        {
            _services.AddTransient<T>(sp => factory(new AspIoCImplementation(sp)));
        }

        public void RegisterSingleton(Type knownType)
        {
            _services.AddSingleton(knownType);
        }

        public void RegisterSingleton(Type interfaceType, Type implementation)
        {
            _services.AddSingleton(interfaceType, implementation);
        }

        public void RegisterSingleton<T>() where T : class
        {
            _services.AddSingleton<T>();
        }

        public void RegisterSingleton<T>(T instance) where T : class
        {
            _services.AddSingleton<T>(instance);
        }

        public void RegisterSingleton<T, TImpl>() where T : class where TImpl : class, T
        {
            _services.AddSingleton<T, TImpl>();
        }

        public void RegisterSingleton<T>(Func<IServiceContainer, T> factory) where T : class
        {
            _services.AddSingleton<T>(sp => factory(new AspIoCImplementation(sp)));
        }

        public void RegisterEnumerable<T, TImpl>() where T : class where TImpl : class, T
        {
            _services.TryAddEnumerable(new ServiceDescriptor(typeof(T), typeof(TImpl), ServiceLifetime.Transient));
        }

        public void Dispose()
        {
        }

        public object Resolve(Type type)
        {
            return _provider.GetRequiredService(type);
        }

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public object TryResolve(Type type)
        {
            return _provider.GetService(type);
        }

        public T TryResolve<T>() where T : class
        {
            return (T) TryResolve(typeof(T));
        }

        public IEnumerable<T> ResolveEnumerable<T>() where T : class
        {
            return _provider.GetServices<T>();
        }

        public IServiceContainer CreateScope()
        {
            throw new NotImplementedException();
        }
    }
}