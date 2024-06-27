using System;

using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs.Components
{
    public class OscillatePosition : Component
    {
        Vector2 _direction = Vector2.UnitX;
        int _time = 0;
        Point oldPosition;

        public OscillatePosition()
        {
            OscillatePositionSystem.Register(this);
        }

        public override void OnCreate()
        {
            oldPosition = Entity.GetComponent<Transform>().position + new Point(-1, 0);
        }

        public override void Update()
        {
            Solid solid = Entity.GetComponent<Solid>();
            if(solid is null || solid.IsDisposed) return;

            Transform transform = Entity.GetComponent<Transform>();
            transform.scale.Y = 2;

            solid.Velocity = Vector2.Round(oldPosition.ToVector2() + new Vector2(MathF.Sin((float)_time / 120 * MathF.PI) * 4) * Vector2.Normalize(_direction)) - transform.position.ToVector2();

            _time++;
        }
    }
}
