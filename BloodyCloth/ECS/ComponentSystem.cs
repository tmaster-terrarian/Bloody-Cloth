using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs;

public abstract class ComponentSystem<T> where T : Component
{
    protected static List<T> _activeComponents = new List<T>();

    public static List<T> Components => _activeComponents;

    public static void Register(T component)
    {
        _activeComponents.Add(component);
    }

    public static void UnRegister(T component)
    {
        _activeComponents.Remove(component);
    }

    public static void UpdateComponents()
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

    public static void DrawComponents()
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

public static class ComponentSystems
{
    public static void Update()
    {
        TransformSystem.UpdateComponents();
        OscillatePositionSystem.UpdateComponents();
        SolidSystem.UpdateComponents();
        ActorSystem.UpdateComponents();
        SpriteSystem.UpdateComponents();
    }

    public static void Draw()
    {
        TransformSystem.DrawComponents();
        OscillatePositionSystem.DrawComponents();
        SolidSystem.DrawComponents();
        ActorSystem.DrawComponents();
        SpriteSystem.DrawComponents();
    }
}

public class OscillatePositionSystem : ComponentSystem<Components.OscillatePosition> {}
public class SolidSystem : ComponentSystem<Components.Solid> {}
public class ActorSystem : ComponentSystem<Components.Actor> {}
public class SpriteSystem : ComponentSystem<Components.Sprite> {}
public class TransformSystem : ComponentSystem<Components.Transform> {}
