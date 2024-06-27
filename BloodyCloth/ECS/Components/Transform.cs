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
            TransformSystem.Register(this);
        }
    }
}
