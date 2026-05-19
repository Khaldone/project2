// Assets/Scripts/CoreDomain/Services/ServiceLocator.cs
using System;
using System.Collections.Generic;
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();


    // VContainer will call this during boot to register the services
    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services[type] = service; // Override for tests
        }
        else
        {
            _services.Add(type, service);
        }
    }


    // Any UI script can call this to get a service safely
    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }
        throw new Exception($"Service {typeof(T)} is not registered!");
    }


    public static void Clear()
    {
        _services.Clear();
    }
}