using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BloodyCloth.Ecs.Systems;
using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs;

public class EntityWorld : IDisposable
{
    private List<Entity> _entities = new();
    private uint _entityID = 0;

    private bool _disposed = false;

    public List<Entity> Entities => _entities;

    public bool IsDisposed => _disposed;

    public Entity Create(Entity entity, Component[]? components = null)
    {
        entity.ID = _entityID++;

        if(components != null)
        foreach(var component in components)
        {
            entity.AddComponent(component);
        }

        this._entities.Add(entity);

        return entity;
    }

    public Entity? GetEntityWithId(uint id)
    {
        foreach(var entity in _entities)
        {
            if(entity.ID == id) return entity;
        }
        return null;
    }

    private void Dispose(bool disposing)
    {
        if(!_disposed)
        {
            if(disposing)
            {
                // managed
            }

            foreach(Entity entity in _entities)
            {
                Destroy(entity);
            }

            _entities.Clear();
            _entities = null;

            _disposed = true;
        }
    }

    ~EntityWorld()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Destroy(Entity entity)
    {
        foreach(var component in entity.Components)
        {
            component.OnDestroy();
        }

        this._entities.Remove(entity);

        entity.Dispose();
    }

    public void Destroy(uint id)
    {
        foreach(Entity entity in _entities)
        {
            if(entity.ID == id)
            {
                Destroy(entity);
            }
        }
    }
}
