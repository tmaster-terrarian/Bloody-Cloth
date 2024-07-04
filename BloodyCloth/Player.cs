using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using BloodyCloth.GameContent;
using BloodyCloth.Graphics;
using BloodyCloth.Utils;

namespace BloodyCloth;

public enum PlayerState
{
    IgnoreState,
    StandIdle,
    Normal,
    Dead
}

public class Player : MoveableEntity
{
    private PlayerState _state = PlayerState.Normal; // please do NOT touch this thx
    private bool _stateJustChanged;
    private readonly List<Texture2D> textures = [];
    private readonly List<int> frameCounts = [];
    private int textureIndex;
    private float frame;
    private bool jumpCancelled = false;
    private bool useGravity = true;
    private bool running;
    private bool fxTrail;
    private int fxTrailCounter;
    private List<AfterImage> afterImages = [];
    private float moveSpeed = 2.5f;
    private float jumpSpeed = -4.5f;
    private float gravity = 0.2f;

    public Point HitboxOffset => new(10, 10);

    public override bool Active => true;

    public PlayerState State {
        get => _state;
        set {
            if(value < 0 || value > PlayerState.Dead) throw new NullReferenceException(nameof(value));

            if(_state != value)
            {
                _stateJustChanged = true;

                OnStateExit(_state);
                OnStateEnter(value);

                _state = value;
            }
        }
    }

    private readonly PlayerInputMapping inputMapping = new PlayerInputMapping {
        // rebind here with Name = Keys
    };

    public PlayerInputMapping InputMapping => inputMapping;

    public bool GamePad { get; set; }

    public bool Visible { get; private set; } = true;

    public PlayerLoadout Loadout { get; private set; } = new();

    int testWeaponCooldown = 0;

    public Player()
    {
        Width = 12;
        Height = 38;

        Bottom = new(100, 104);

        string texPath = "Images/Player/";
        AddTexture(Main.GetContent<Texture2D>(texPath + "idle"));
        AddTexture(Main.GetContent<Texture2D>(texPath + "run"), 2);
    }

    private void AddTexture(Texture2D texture, int frameCount = 1)
    {
        textures.Add(texture);
        frameCounts.Add(frameCount);
    }

    private void OnStateEnter(PlayerState state)
    {
        switch(state)
        {
            case PlayerState.IgnoreState:
                break;
            case PlayerState.StandIdle:
                break;
            case PlayerState.Normal:
                break;
            case PlayerState.Dead:
                break;
            default:
                break;
        }
    }

    public void Update()
    {
        int inputDir = Input.GetDown(InputMapping.KeyRight).ToInt32() - Input.GetDown(InputMapping.KeyLeft).ToInt32();

        bool wasOnGround = OnGround;
        bool onJumpthrough = CheckCollidingJumpthrough(BottomEdge.Shift(0, 1));
        if(onJumpthrough) OnGround = true;
        else OnGround = CheckColliding(BottomEdge.Shift(0, 1));

        if(!wasOnGround && OnGround)
        {
            jumpCancelled = false;
        }

        float accel = 0.15f;
        float fric = 0.1f;
        if(!OnGround)
        {
            accel = 0.07f;
            fric = 0.05f;
        }

        if(!_stateJustChanged)
        {
            useGravity = false;
            CollidesWithJumpthroughs = true;
            CollidesWithSolids = true;
        }
        else _stateJustChanged = false;

        switch(State)
        {
            case PlayerState.IgnoreState:
                break;
            case PlayerState.StandIdle:
                useGravity = true;

                velocity.X = MathUtil.Approach(velocity.X, 0, fric * 2);

                textureIndex = 0;

                break;
            case PlayerState.Normal:
                useGravity = true;

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
                    if(Input.GetReleased(InputMapping.KeyJump) && velocity.Y < 0 && !jumpCancelled)
                    {
                        jumpCancelled = true;
                        velocity.Y /= 2;
                    }
                }
                else
                {
                    if(onJumpthrough && Input.GetDown(InputMapping.KeyDown) && !CheckColliding(BottomEdge.Shift(new(0, 2)), true))
                    {
                        position.Y += 2;
                        onJumpthrough = CheckCollidingJumpthrough(BottomEdge.Shift(0, 1));
                        if(onJumpthrough) OnGround = true;
                        else OnGround = CheckColliding(BottomEdge.Shift(0, 1));
                    }
                }

                if(running)
                {
                    frame += Math.Abs(velocity.X) / frameCounts[textureIndex] / 8;
                }

                fxTrail = Math.Abs(velocity.X) > 1f * moveSpeed;

                // ...

                if(OnGround && Input.GetPressed(InputMapping.KeyJump))
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

        if(testWeaponCooldown > 0) testWeaponCooldown = MathUtil.Approach(testWeaponCooldown, 0, 1);

        if(Input.GetDown(MouseButtons.LeftButton) && testWeaponCooldown == 0)
        {
            testWeaponCooldown = 3;

            Vector2 vel = this.DirectionTo(Main.WorldMousePosition);
            if(Main.WorldMousePosition == Center) vel = Vector2.UnitX * Facing;

            Projectile.Create(ProjectileType.CrossbowBolt, Center - new Point(4), vel * 10);
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
            if(!(Input.GetDown(InputMapping.KeyDown) && CheckCollidingJumpthrough(BottomEdge.Shift(new(0, 1)))))
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

    private void OnStateExit(PlayerState state)
    {
        
    }

    public void Draw()
    {
        foreach(var image in afterImages)
        {
            if(!Visible) continue;

            var texture2 = textures[image.TextureIndex];
            int width2 = texture2.Width / frameCounts[image.TextureIndex];
            Rectangle drawFrame2 = new(image.Frame * width2, 0, width2, texture2.Height);

            if(Main.Debug.Enabled)
            {
                NineSlice.DrawNineSlice(
                    Main.GetContent<Texture2D>("Images/Other/tileOutline"),
                    new Rectangle(image.Position.X, image.Position.Y, Width, Height),
                    null,
                    new Point(1),
                    new Point(1),
                    Color.Blue * 0.75f
                );
            }

            Renderer.SpriteBatch.Draw(
                texture2,
                (image.Position + new Point(this.Width / 2, this.Height / 2)).ToVector2() - new Vector2(0, HitboxOffset.Y / 2),
                drawFrame2,
                image.Color * (image.Alpha * 0.5f),
                image.Rotation,
                new Vector2(width2 / 2, texture2.Height / 2),
                image.Scale,
                image.Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );
        }

        if(Main.Debug.Enabled && Main.Debug.DrawTileCheckingAreas)
        {
            Rectangle newRect = new Rectangle
            {
                X = Extensions.Floor(position.X * 0.125f),
                Y = Extensions.Floor(position.Y * 0.125f)
            };
            newRect.Width = MathHelper.Max(1, Extensions.Ceiling(Width * 0.125f) + (Extensions.Floor((position.X + 4) * 0.125f) - newRect.X));
            newRect.Height = MathHelper.Max(1, Extensions.Ceiling(Height * 0.125f) + (Extensions.Floor((position.Y + 4) * 0.125f) - newRect.Y));

            for(int x = newRect.X; x < newRect.X + newRect.Width; x++)
            {
                for(int y = newRect.Y; y < newRect.Y + newRect.Height; y++)
                {
                    NineSlice.DrawNineSlice(Main.GetContent<Texture2D>("Images/Other/tileOutline"), new Rectangle(x, y, 1, 1).ScalePosition(8), null, new Point(1), new Point(1), Color.LimeGreen * 0.75f);
                }
            }
        }

        if(Visible)
        {
            var texture = textures[textureIndex];
            int width = texture.Width / frameCounts[textureIndex];
            Rectangle drawFrame = new((int)frame * width, 0, width, texture.Height);

            Renderer.SpriteBatch.Draw(
                texture,
                Center.ToVector2() - new Vector2(0, HitboxOffset.Y / 2),
                drawFrame,
                Color,
                Rotation,
                new Vector2(width / 2, texture.Height / 2),
                drawScale,
                Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );
        }

        if(Main.Debug.Enabled)
        {
            NineSlice.DrawNineSlice(Main.GetContent<Texture2D>("Images/Other/tileOutline"), Hitbox, null, new Point(1), new Point(1), Color.Red * 0.5f);
        }
    }

    public bool IsRiding(Ecs.Components.Solid solid)
    {
        return solid.Collidable && Hitbox.Shift(new Point(0, 1)).Intersects(solid.WorldBoundingBox);
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

public class PlayerLoadout
{
    private bool selectedSecondary = false;

    public WeaponType PrimaryWeapon { get; set; } = WeaponType.Invalid;
    public WeaponType SecondaryWeapon { get; set; } = WeaponType.Invalid;

    public SpellType[] Spells { get; } = new SpellType[3];

    public bool HasWeapon => PrimaryWeapon > WeaponType.Invalid || SecondaryWeapon > WeaponType.Invalid;

    public bool HasSpell => Spells[0] > SpellType.Invalid || Spells[1] > SpellType.Invalid || Spells[2] > SpellType.Invalid;
}
