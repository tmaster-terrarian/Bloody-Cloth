using System;
using BloodyCloth.Utils;
using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs.Components
{
    [Serializable]
    public class Actor : Component
    {
        Rectangle bbox = Rectangle.Empty;

        Transform transform;

        float xRemainder;
        float yRemainder;

        Vector2 velocity = Vector2.Zero;

        public Rectangle BoundingBox
        {
            get => bbox;
            set {
                this.bbox = new(value.Location - (Entity?.GetComponent<Sprite>()?.origin ?? Point.Zero), value.Size);
            }
        }

        public Rectangle WorldBoundingBox
        {
            get => new(bbox.X + transform?.position.X ?? bbox.X, bbox.Y + transform?.position.Y ?? bbox.Y, bbox.Width, bbox.Height);
        }

        public Vector2 Velocity { get => velocity; set => velocity = value; }

        public bool DefaultBehavior { get; set; }

        public Actor()
        {
            Systems.ActorSystem.Register(this);
        }

        public override void OnCreate()
        {
            transform = Entity.GetComponent<Transform>();

            Point origin = Entity.GetComponent<Sprite>()?.origin ?? Point.Zero;
            bbox.X -= origin.X;
            bbox.Y -= origin.Y;
        }

        public override void Update()
        {
            if(!DefaultBehavior) return;

            MoveX(velocity.X, () => {
                velocity.X = 0;
            });
            MoveY(velocity.Y, () => {
                velocity.Y = 0;
            });
        }

        public override void Draw()
        {
            Main.World.DrawSprite(Main.OnePixel, WorldBoundingBox, null, Color.Red * 0.5f, 0, Vector2.Zero, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
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
                    if(
                        !Main.World.TileMeeting(WorldBoundingBox.Shift(new(sign, 0)))
                        && !Main.World.SolidMeeting(WorldBoundingBox, new(sign, 0))
                    )
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
                    if(
                        !Main.World.TileMeeting(WorldBoundingBox.Shift(new(0, sign)))
                        && !Main.World.SolidMeeting(WorldBoundingBox, new(0, sign))
                    )
                    {
                        transform.position.Y += sign;
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
            return solid.Collidable && new Rectangle(WorldBoundingBox.Location + new Point(0, 1), bbox.Size).Intersects(solid.WorldBoundingBox);
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
