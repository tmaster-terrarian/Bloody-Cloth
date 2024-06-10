using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs;

public class Entity : Toggleable, IDisposable
{
    public uint ID { get; set; }

    private bool _disposed = false;

    public bool IsDisposed => _disposed;

    private List<string> _tags = new();

    public List<string> Tags => _tags;

    private List<Component> _components = new() {
        new Components.Transform()
    };

    public List<Component> Components => _components;

    public T? GetComponent<T>() where T : Component
    {
        if(_disposed) throw new ObjectDisposedException(nameof(Components));

        foreach(var component in this._components)
        {
            if(component.GetType().Equals(typeof(T))) return (T)component;
        }
        return null;
    }

    public bool HasComponent(Type type)
    {
        if(_disposed) throw new ObjectDisposedException(nameof(Components));

        foreach(var component in this._components)
        {
            if(component.GetType().Equals(type)) return true;
        }
        return false;
    }

    public bool HasComponent<T>() where T : Component
    {
        if(_disposed) throw new ObjectDisposedException(nameof(Components));

        foreach(var component in this._components)
        {
            if(component.GetType().Equals(typeof(T))) return true;
        }
        return false;
    }

    public T AddComponent<T>(T component) where T : Component
    {
        if(_disposed) throw new ObjectDisposedException(nameof(Components));

        if(this.HasComponent(component.GetType()))
        {
            throw new Exception("Cannot add component to the entity because it already has a component with the same type (" + component.GetType().FullName + ")");
        }

        _components.Add(component);
        component.Entity = this;

        component.OnCreate();

        return component;
    }

    ~Entity()
    {
        Dispose(false);
    }

    protected virtual void CleanupManaged() {}

    protected virtual void CleanupUnmanaged() {}

    private void Dispose(bool disposing)
    {
        if(!_disposed)
        {
            if(disposing)
            {
                this.CleanupManaged();

                _tags.Clear();
                _tags = null;
            }

            foreach(var component in _components)
            {
                component.Dispose();
            }
            _components.Clear();
            _components = null;

            this.CleanupUnmanaged();

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
