using System;

using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs.Components
{
    [Serializable]
    public class Actor : Component
    {
        Rectangle bbox;

        Transform transform;

        float xRemainder;
        float yRemainder;

        Vector2 velocity = Vector2.Zero;

        public Rectangle BoundingBox => bbox;

        public Rectangle WorldBoundingBox => new(bbox.X + transform?.position.X ?? bbox.X, bbox.Y + transform?.position.Y ?? bbox.Y, bbox.Width, bbox.Height);

        public Vector2 Velocity { get => velocity; set => velocity = value; }

        public Actor()
        {
            Systems.ActorSystem.Register(this);
        }

        public override void OnCreate()
        {
            if(Entity.HasComponent<Solid>()) throw new Exception("Actors and Solids are mutually exclusive");

            transform = Entity.GetComponent<Transform>();

            Point origin = Entity.GetComponent<Sprite>()?.origin ?? Point.Zero;
            bbox.X -= origin.X;
            bbox.Y -= origin.Y;
        }

        public void MoveX(float amount, Action? onCollide)
        {
            xRemainder += amount;
            int move = Extensions.Round(xRemainder);
            if(move != 0)
            {
                xRemainder -= move;
                int sign = Math.Sign(move);
                while(move != 0)
                {
                    if(!Main.World.TileMeeting(new(transform.position + new Point(bbox.X + sign, 0), bbox.Size)))
                    {
                        transform.position.X += sign;
                        move -= sign;
                        continue;
                    }

                    onCollide?.Invoke();
                    break;
                }
            }
        }

        public void MoveY(float amount, Action? onCollide)
        {
            yRemainder += amount;
            int move = Extensions.Round(yRemainder);
            if(move != 0)
            {
                yRemainder -= move;
                int sign = Math.Sign(move);
                while(move != 0)
                {
                    if(!Main.World.TileMeeting(new(transform.position + new Point(0, bbox.Y + sign), bbox.Size)))
                    {
                        transform.position.X += sign;
                        move -= sign;
                        continue;
                    }

                    onCollide?.Invoke();
                    break;
                }
            }
        }

        public virtual bool IsRiding(Solid solid)
        {
            return new Rectangle(WorldBoundingBox.Location + new Point(0, 1), bbox.Size).Intersects(solid.WorldBoundingBox);
        }

        public virtual void Squish()
        {
            Main.Logger.LogInfo("Actor " + Entity.ID + " was squished");
        }
    }
}

namespace BloodyCloth.Ecs.Systems
{
    public class ActorSystem : ComponentSystem<Components.Actor>
    {
        
    }
}
