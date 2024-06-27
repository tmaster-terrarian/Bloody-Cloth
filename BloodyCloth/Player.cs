using System;
using System.Collections.Generic;
using BloodyCloth.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BloodyCloth;

public enum PlayerState
{
    Normal,
    DoNothing,
    Dead
}

public class Player : Entity
{
    public class PlayerInputMapping
    {
        public Keys Right { get; set; } = Keys.D;
        public Keys Left { get; set; } = Keys.A;
        public Keys Down { get; set; } = Keys.S;
        public Keys Up { get; set; } = Keys.W;
        public Keys Jump { get; set; } = Keys.Space;
    }

    private readonly List<Texture2D> textures = new();
    private readonly List<int> frameCounts = new();
    private int textureIndex;
    private float frame;
    private bool jumpCancelled = false;
    private float xRemainder;
    private float yRemainder;
    private bool useGravity = true;
    private bool running;
    private bool fxTrail;
    private int fxTrailCounter;
    private List<AfterImage> afterImages = new();
    private float moveSpeed = 2.5f;
    private float jumpSpeed = -4.5f;
    private float gravity = 0.2f;

    public override bool Active => true;

    public PlayerState State { get; private set; } = PlayerState.Normal;

    public PlayerInputMapping InputMapping { get; } = new PlayerInputMapping {
        // rebind here with Name = Keys
    };

    public bool OnGround { get; set; }

    public bool GamePad { get; set; }

    public bool Visible { get; private set; } = true;

    public Player()
    {
        Width = 16;
        Height = 26;

        Bottom = new(100, 104);

        string texPath = "Images/Player/";
        AddTexture(Main.GetContent<Texture2D>(texPath + "idle"));
        AddTexture(Main.GetContent<Texture2D>(texPath + "run"), 2);
    }

    void AddTexture(Texture2D texture, int frameCount = 1)
    {
        textures.Add(texture);
        frameCounts.Add(frameCount);
    }

    static bool CheckColliding(Rectangle rectangle)
    {
        return Main.World.TileMeeting(rectangle) || Main.World.SolidMeeting(rectangle);
    }

    public void Update()
    {
        int inputDir = Input.GetDown(InputMapping.Right).ToInt32() - Input.GetDown(InputMapping.Left).ToInt32();

        bool wasOnGround = OnGround;
        bool onJumpthrough = Main.World.JumpThroughMeeting(Hitbox.Shift(new(0, 1))) && !Main.World.JumpThroughMeeting(Hitbox);
        OnGround = CheckColliding(Hitbox.Shift(new(0, 1))) || onJumpthrough;

        if(!wasOnGround && OnGround)
        {
            jumpCancelled = false;
        }

        switch(State)
        {
            case PlayerState.Normal:
                useGravity = true;

                float accel = 0.15f;
                float fric = 0.1f;
                if(!OnGround)
                {
                    accel = 0.07f;
                    fric = 0.05f;
                }

                if(inputDir != 0)
                {
                    Facing = inputDir;

                    if(OnGround)
                    {
                        running = true;

                        if(velocity.Y >= 0)
                        {
                            textureIndex = 1;
                        }
                    }

                    if(inputDir * velocity.X < 0)
                    {
                        velocity.X = MathUtil.Approach(velocity.X, 0, fric);
                    }
                    if(inputDir * velocity.X < moveSpeed)
                    {
                        velocity.X = MathUtil.Approach(velocity.X, inputDir * moveSpeed, accel);
                    }
                    if(inputDir * velocity.X > moveSpeed && OnGround)
                    {
                        velocity.X = MathUtil.Approach(velocity.X, inputDir * moveSpeed, fric/2);
                    }
                }
                else
                {
                    running = false;
                    velocity.X = MathUtil.Approach(velocity.X, 0, fric * 2);

                    if(Math.Abs(velocity.X) < 1.5f && OnGround)
                    {
                        textureIndex = 0;
                    }
                }

                if(!OnGround)
                {
                    if(Input.GetReleased(InputMapping.Jump) && velocity.Y < 0 && !jumpCancelled)
                    {
                        jumpCancelled = true;
                        velocity.Y /= 2;
                    }
                }
                else
                {
                    if(onJumpthrough && Input.GetDown(InputMapping.Down))
                    {
                        position.Y++;
                        velocity.Y = gravity;
                    }
                }

                if(running && !CheckColliding(Hitbox.Shift(inputDir, 0)))
                {
                    frame += Math.Abs(velocity.X) / frameCounts[textureIndex] / 8;
                }

                fxTrail = Math.Abs(velocity.X) > 1f * moveSpeed;

                // ...

                if(OnGround && Input.GetPressed(InputMapping.Jump))
                {
                    velocity.Y = jumpSpeed;
                }

                break;
        }

        if(!OnGround && useGravity)
        {
            if(velocity.Y >= 0.1)
                velocity.Y = MathUtil.Approach(velocity.Y, 20, gravity);
            if(velocity.Y < 0)
                velocity.Y = MathUtil.Approach(velocity.Y, 20, gravity);
            else if (velocity.Y < 2)
                velocity.Y = MathUtil.Approach(velocity.Y, 20, gravity * 0.25f);
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

        if(fxTrail)
        {
            fxTrailCounter++;
            if(fxTrailCounter >= 2)
            {
                fxTrailCounter = 0;
                afterImages.Add(new AfterImage {
                    TextureIndex = textureIndex,
                    Frame = (int)frame,
                    Position = position,
                    Facing = Facing,
                    Scale = drawScale,
                    Color = Color,
                    Rotation = Rotation
                });
            }
        }
        else
        {
            fxTrailCounter = 0;
        }

        for(int i = 0; i < afterImages.Count; i++)
        {
            AfterImage image = afterImages[i];
            image.Alpha = MathHelper.Max(image.Alpha - (1f / 12f), 0);
            if(image.Alpha == 0)
            {
                afterImages.RemoveAt(i);
                i--;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach(var image in afterImages)
        {
            if(!Visible) continue;

            var texture2 = textures[image.TextureIndex];
            int width2 = texture2.Width / frameCounts[image.TextureIndex];
            Rectangle drawFrame2 = new(image.Frame * width2, 0, width2, texture2.Height);

            spriteBatch.Draw(
                texture2,
                (image.Position + new Point(this.Width / 2, this.Height / 2)).ToVector2() - Vector2.UnitY * 3,
                drawFrame2,
                image.Color * (image.Alpha * 0.5f),
                image.Rotation,
                new Vector2(width2 / 2, texture2.Height / 2),
                image.Scale,
                image.Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );
        }

        if(Main.DebugMode)
        {
            spriteBatch.Draw(Main.OnePixel, Hitbox, Color.Red * 0.5f);
        }

        if(!Visible) return;

        var texture = textures[textureIndex];
        int width = texture.Width / frameCounts[textureIndex];
        Rectangle drawFrame = new((int)frame * width, 0, width, texture.Height);

        spriteBatch.Draw(
            texture,
            Center.ToVector2() - Vector2.UnitY * 3,
            drawFrame,
            Color,
            Rotation,
            new Vector2(width / 2, texture.Height / 2),
            drawScale,
            Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0
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
                bool col1 = CheckColliding(Hitbox.Shift(new(sign, 0)));
                if((col1 && !CheckColliding(Hitbox.Shift(new(sign, -1)))) || (Main.World.JumpThroughMeeting(Hitbox.Shift(new(sign, 0))) && !Main.World.JumpThroughMeeting(Hitbox.Shift(new(sign, -1)))))
                {
                    position.X += sign;
                    position.Y -= 1;
                    move -= sign;
                }
                else if(!col1)
                {
                    if(OnGround)
                    {
                        if(!CheckColliding(Hitbox.Shift(new(sign, 1))) && !Main.World.JumpThroughMeeting(Hitbox.Shift(new(sign, 1))) && (CheckColliding(Hitbox.Shift(new(sign, 2))) || Main.World.JumpThroughMeeting(Hitbox.Shift(new(sign, 2)))))
                            position.Y += 1;
                    }
                    position.X += sign;
                    move -= sign;
                }
                else
                {
                    onCollide?.Invoke();
                    break;
                }
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
                if(!CheckColliding(Hitbox.Shift(new(0, sign))) && !(sign == 1 && Main.World.JumpThroughMeeting(Hitbox.Shift(new(0, sign))) && !Main.World.JumpThroughMeeting(Hitbox)))
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

    public class AfterImage
    {
        public float Alpha = 1;
        public int TextureIndex;
        public int Frame;
        public Point Position;
        public int Facing;
        public Vector2 Scale = Vector2.One;
        public Color Color = Color.White;
        public float Rotation;
    }
}
