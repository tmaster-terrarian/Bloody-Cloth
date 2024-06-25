using System;
using System.Collections.Generic;
using BloodyCloth.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BloodyCloth;

public class Player : Entity
{
    public override bool Active => true;

    List<Texture2D> textures = null;

    public class PlayerInputMapping
    {
        public Keys Right { get; set; } = Keys.D;
        public Keys Left { get; set; } = Keys.A;
        public Keys Down { get; set; } = Keys.S;
        public Keys Up { get; set; } = Keys.W;
        public Keys Jump { get; set; } = Keys.Space;
    }

    private PlayerInputMapping _inputMapping = new PlayerInputMapping {
        // rebind here with Name = Keys
    };

    private bool _onGround = false;
    private bool jumpCancelled = false;
    private float xRemainder;
    private float yRemainder;

    public float moveSpeed = 2;
    public float jumpSpeed = -4f;
    public float gravity = 0.2f;

    public PlayerInputMapping InputMapping => _inputMapping;

    public bool OnGround { get => _onGround; set => _onGround = value; }

    public bool Controller { get; set; }

    public Player()
    {
        textures = new() {
            Main.GetContent<Texture2D>("Images/player")
        };

        width = 16;
        height = 24;

        Bottom = new(100, 104);
    }

    public void Update()
    {
        int inputDir = Input.GetDown(_inputMapping.Right).ToInt32() - Input.GetDown(_inputMapping.Left).ToInt32();

        bool wasOnGround = _onGround;
        _onGround = Main.World.TileMeeting(Hitbox.Shift(new(0, 1))) || Main.World.SolidMeeting(Hitbox, new(0, 1));

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
                velocity.Y = MathUtil.Approach(velocity.Y, 20, gravity);
            if(velocity.Y < 0)
                velocity.Y = MathUtil.Approach(velocity.Y, 20, gravity);
            else if (velocity.Y < 2)
                velocity.Y = MathUtil.Approach(velocity.Y, 20, gravity * 0.25f);

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

        if(Input.GetDown(Keys.LeftControl))
        {
            velocity.Y = 0;
            velocity.X = 0;
            Center = Main.WorldMousePosition;
        }

        MoveX(velocity.X, () => {
            velocity.X = 0;
        });
        MoveY(velocity.Y, () => {
            velocity.Y = 0;
        });
    }

    public void Draw()
    {
        Main.World.DrawSprite(
            Main.OnePixel,
            new Rectangle(position, Size),
            null,
            Color.Red * 0.5f,
            rotation,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );

        Main.World.DrawSprite(
            textures[0],
            position.ToVector2(),
            null,
            color,
            rotation,
            new Vector2(8, 8),
            drawScale,
            facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            LayerDepth
        );
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
                    !Main.World.TileMeeting(Hitbox.Shift(new(sign, 0)))
                    && !Main.World.SolidMeeting(Hitbox, new(sign, 0))
                )
                {
                    position.X += sign;
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
                    !Main.World.TileMeeting(Hitbox.Shift(new(0, sign)))
                    && !Main.World.SolidMeeting(Hitbox, new(0, sign))
                )
                {
                    position.Y += sign;
                    move -= sign;
                    continue;
                }

                onCollide?.Invoke();
                break;
            }
        }
    }

    public bool IsRiding(Ecs.Components.Solid solid)
    {
        return solid.Collidable && Hitbox.Shift(new Point(0, 2)).Intersects(solid.WorldBoundingBox);
    }

    public void Squish()
    {
        Main.Logger.LogInfo("Player was squished!");
    }
}
