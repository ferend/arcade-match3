using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void RegisterService<T>(T service)
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                throw new InvalidOperationException($"Service {type.Name} is already registered.");
            }
            services[type] = service;
        }

        public static T GetService<T>()
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service {type.Name} not found.");
        }
        public static void DisposeService<T>()
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var service))
            {
                if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                services.Remove(type);
            }
        }
    }

}