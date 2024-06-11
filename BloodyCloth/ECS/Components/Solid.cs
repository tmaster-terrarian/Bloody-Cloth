using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs.Components
{
    [Serializable]
    public class Solid : Component
    {
        Rectangle bbox;

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

        public Rectangle WorldBoundingBox => new(bbox.X + transform?.position.X ?? bbox.X, bbox.Y + transform?.position.Y ?? bbox.Y, bbox.Width, bbox.Height);

        public Vector2 Velocity { get => velocity; set => velocity = value; }

        public bool Collidable { get; protected set; }

        public bool DefaultBehavior { get; set; }

        public Solid()
        {
            Systems.SolidSystem.Register(this);
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

            Move(velocity.X, velocity.Y);
        }

        public void Move(float x, float y)
        {
            xRemainder += x;
            yRemainder += y;   
            int moveX = Extensions.Round(xRemainder);
            int moveY = Extensions.Round(yRemainder);
            if(moveX != 0 || moveY != 0)
            {
                // Loop through every Actor in the Level, add it to
                // a list if actor.IsRiding(this) is true
                var riding = GetAllRidingActors();
                var allActors = Main.World.GetAllEntitiesWithComponent<Actor>();

                // Make this Solid non-collidable for Actors,
                // so that Actors moved by it do not get stuck on it
                Collidable = false;

                if(moveX != 0)
                {
                    xRemainder -= moveX;
                    transform.position.X += moveX;
                    if(moveX > 0)
                    {
                        foreach(var entity in allActors)
                        {
                            var actor = entity.GetComponent<Actor>();
                            if(this.WorldBoundingBox.Intersects(actor.WorldBoundingBox))
                            {
                                // Push right
                                actor.MoveX(this.WorldBoundingBox.Right - actor.WorldBoundingBox.Left, actor.Squish);
                            }
                            else if(riding.Contains(actor))
                            {
                                // Carry right
                                actor.MoveX(moveX, null);
                            }
                        }
                    }
                    else
                    {
                        foreach(var entity in allActors)
                        {
                            var actor = entity.GetComponent<Actor>();
                            if(this.WorldBoundingBox.Intersects(actor.WorldBoundingBox))
                            {
                                // Push left
                                actor.MoveX(this.WorldBoundingBox.Left - actor.WorldBoundingBox.Right, actor.Squish);
                            }
                            else if(riding.Contains(actor))
                            {
                                // Carry left
                                actor.MoveX(moveX, null);
                            }
                        }
                    }
                }

                if(moveY != 0)
                {
                    yRemainder -= moveY;
                    transform.position.Y += moveY;
                    if(moveY > 0)
                    {
                        foreach(var entity in allActors)
                        {
                            var actor = entity.GetComponent<Actor>();
                            if(this.WorldBoundingBox.Intersects(actor.WorldBoundingBox))
                            {
                                // Push right
                                actor.MoveY(this.WorldBoundingBox.Bottom - actor.WorldBoundingBox.Top, actor.Squish);
                            }
                            else if(riding.Contains(actor))
                            {
                                // Carry right
                                actor.MoveY(moveY, null);
                            }
                        }
                    }
                    else
                    {
                        foreach(var entity in allActors)
                        {
                            var actor = entity.GetComponent<Actor>();
                            if(this.WorldBoundingBox.Intersects(actor.WorldBoundingBox))
                            {
                                // Push left
                                actor.MoveY(this.WorldBoundingBox.Top - actor.WorldBoundingBox.Bottom, actor.Squish);
                            }
                            else if(riding.Contains(actor))
                            {
                                // Carry left
                                actor.MoveY(moveY, null);
                            }
                        }
                    }
                }

                // Re-enable collisions for this Solid
                Collidable = true;
            }
        }

        private List<Actor> GetAllRidingActors()
        {
            List<Actor> actors = new();
            foreach(var entity in Main.World.GetAllEntitiesWithComponent<Actor>())
            {
                var actor = entity.GetComponent<Actor>();
                if(actor.IsRiding(this)) actors.Add(actor);
            }
            return actors;
        }
    }
}

namespace BloodyCloth.Ecs.Systems
{
    public class SolidSystem : ComponentSystem<Components.Solid>
    {
        
    }
}
