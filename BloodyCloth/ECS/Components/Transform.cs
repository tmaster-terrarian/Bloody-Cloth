using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs.Components
{
    [System.Serializable]
    public class Transform : Component
    {
        public Point position = Point.Zero;
        public Vector2 scale = Vector2.One;
        public float rotation = 0;

        public Transform()
        {
            Systems.TransformSystem.Register(this);
        }
    }
}

namespace BloodyCloth.Ecs.Systems { public class TransformSystem : ComponentSystem<Components.Transform> {} }
