using System.Collections.Generic;
using AsepriteDotNet;
using BloodyCloth.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BloodyCloth.Ecs.Components
{
    [System.Serializable]
    public class PlayerBehavior : Component
    {
        List<Texture2D> textures = null;

        public class PlayerInputMapping
        {
            public Keys Right { get; set; } = Keys.D;
            public Keys Left { get; set; } = Keys.A;
            public Keys Down { get; set; } = Keys.S;
            public Keys Up { get; set; } = Keys.W;
            public Keys Jump { get; set; } = Keys.Space;
        }

        public PlayerIndex ControllerIndex { get; set; }
        public bool Controller { get; set; } = false;

        private PlayerInputMapping _inputMapping = new PlayerInputMapping {
            // rebind here with Name = Keys
        };

        private float moveSpeed = 2;
        private float jumpSpeed = -4f;
        private int facing = 1;
        private bool _onGround = false;
        private bool jumpCancelled = false;

        public const float Gravity = 0.2f;

        public PlayerInputMapping InputMapping => _inputMapping;

        public bool OnGround { get => _onGround; set => _onGround = value; }

        public PlayerBehavior()
        {
            Systems.PlayerBehaviorSystem.Register(this);
        }

        public static Entity CreatePlayerEntity(PlayerIndex index)
        {
            return Main.World.Entities.Create(new Component[] {
                new PlayerBehavior {
                    ControllerIndex = index,
                },
            });
        }

        public override void OnCreate()
        {
            textures = new List<Texture2D> {
                Main.GetContent<Texture2D>("Images/player")
            };

            Entity.GetComponent<Transform>().position = new(100, 104);

            Entity.AddComponent(new Sprite {
                texture = textures[0],
                origin = new Point(16, 32)
            });

            var actor = Entity.AddComponent(new Actor {
                DefaultBehavior = true,
            });
            actor.BoundingBox = new(8, 8, 16, 24);
        }

        public override void Update()
        {
            Actor actor = Entity.GetComponent<Actor>();

            int inputDir = Input.GetDown(_inputMapping.Right).ToInt32() - Input.GetDown(_inputMapping.Left).ToInt32();

            bool wasOnGround = _onGround;
            _onGround = Main.World.TileMeeting(actor.WorldBoundingBox.Shift(new(0, 1)));

            if(!wasOnGround && _onGround)
            {
                jumpCancelled = false;
            }

            float accel = 0.15f;
            float fric = 0.1f;
            if(!_onGround)
            {
                accel = 0.07f;
                fric = 0.05f;
            }

            Vector2 velocity = actor.Velocity;

            if(inputDir == 1)
            {
                facing = 1;
                if(velocity.X < 0)
                {
                    velocity.X = MathUtil.Approach(velocity.X, 0, fric);
                }
                if(velocity.X < moveSpeed)
                {
                    velocity.X = MathUtil.Approach(velocity.X, moveSpeed, accel);
                }
                if(velocity.X > moveSpeed && _onGround)
                {
                    velocity.X = MathUtil.Approach(velocity.X, moveSpeed, fric/2);
                }
            }
            else if(inputDir == -1)
            {
                facing = -1;
                if(-velocity.X < 0)
                {
                    velocity.X = MathUtil.Approach(velocity.X, 0, fric);
                }
                if(-velocity.X < moveSpeed)
                {
                    velocity.X = MathUtil.Approach(velocity.X, -moveSpeed, accel);
                }
                if(-velocity.X > moveSpeed && _onGround)
                {
                    velocity.X = MathUtil.Approach(velocity.X, -moveSpeed, fric/2);
                }
            }
            else
            {
                velocity.X = MathUtil.Approach(velocity.X, 0, fric * 2);
            }

            if(!_onGround)
            {
                if(velocity.Y >= 0.1)
                    velocity.Y = MathUtil.Approach(velocity.Y, 20, Gravity);
                if(velocity.Y < 0)
                    velocity.Y = MathUtil.Approach(velocity.Y, 20, Gravity);
                else if (velocity.Y < 2)
                    velocity.Y = MathUtil.Approach(velocity.Y, 20, Gravity * 0.25f);

                if(Input.GetReleased(InputMapping.Jump) && velocity.Y < 0 && !jumpCancelled)
                {
                    jumpCancelled = true;
                    velocity.Y /= 2;
                }
            }
            else if(Input.GetPressed(InputMapping.Jump))
            {
                velocity.Y = jumpSpeed;
            }

            Sprite sprite = Entity.GetComponent<Sprite>();
            if(sprite is not null)
            {
                sprite.spriteEffects = facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            if(Input.GetDown(Keys.LeftControl))
            {
                velocity.Y = 0;
                velocity.X = 0;
                Entity.GetComponent<Transform>().position = Main.MousePosition;
            }

            actor.Velocity = velocity;
        }

        protected override void CleanupUnmanaged()
        {
            foreach(var tex in textures)
            {
                tex.Dispose();
            }
            textures.Clear();
            textures = null;
        }
    }
}

namespace BloodyCloth.Ecs.Systems
{
    public class PlayerBehaviorSystem : ComponentSystem<Components.PlayerBehavior>
    {
    }
}
