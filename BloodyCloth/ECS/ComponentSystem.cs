using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs;

public abstract class ComponentSystem<T> where T : Component
{
    protected static List<T> _activeComponents = new List<T>();

    public static void Register(T component)
    {
        _activeComponents.Add(component);
    }

    public static void UnRegister(T component)
    {
        _activeComponents.Remove(component);
    }

    public static void Update()
    {
        foreach (T component in _activeComponents)
        {
            if(component.IsDisposed)
            {
                UnRegister(component);
                continue;
            }

            if(component.IsEnabled) component.Update();
        }
    }

    public static void Draw()
    {
        foreach (T component in _activeComponents)
        {
            if(component.IsDisposed)
            {
                UnRegister(component);
                continue;
            }

            if(component.IsEnabled) component.Draw();
        }
    }
}
