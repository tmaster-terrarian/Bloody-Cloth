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
    PlayerState _state = PlayerState.Normal; // please do NOT touch this thx
    bool _stateJustChanged;
    readonly List<Texture2D> textures = [];
    readonly List<int> frameCounts = [];
    int textureIndex;
    float frame;
    bool jumpCancelled = false;
    bool useGravity = true;
    bool running;
    bool fxTrail;
    int fxTrailCounter;
    List<AfterImage> afterImages = [];
    float moveSpeed = 2.5f;
    float jumpSpeed = -4.5f;
    float gravity = 0.2f;

    readonly PlayerInputMapping inputMapping = new PlayerInputMapping {
        // rebind here with Name = Keys
    };

    public Point HitboxOffset => new(0, 5);

    public PlayerState State {
        get => _state;
        set {
            if(value < 0 || value > PlayerState.Dead) value = PlayerState.IgnoreState;

            if(_state != value)
            {
                _stateJustChanged = true;

                OnStateExit(_state);
                OnStateEnter(value);

                _state = value;
            }
        }
    }

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

        LayerDepth = 50;

        string texPath = "Images/Player/";
        AddTexture(Main.GetContent<Texture2D>(texPath + "idle"));
        AddTexture(Main.GetContent<Texture2D>(texPath + "run"), 2);
    }

    void AddTexture(Texture2D texture, int frameCount = 1)
    {
        textures.Add(texture);
        frameCounts.Add(frameCount);
    }

    void OnStateEnter(PlayerState state)
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
        else
        {
            _stateJustChanged = false;
        }

        switch (State)
        {
            case PlayerState.StandIdle:
                useGravity = true;

                velocity.X = MathUtil.Approach(velocity.X, 0, fric * 2);

                if(OnGround)
                {
                    textureIndex = 0;
                }

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
            case PlayerState.IgnoreState: default:
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

            Projectile.Create(ProjectileType.CrossbowBolt, Center - new Point(4), vel * 10, LayerDepth + 1);
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
        }
    }

    public void Draw()
    {
        foreach(var image in afterImages)
        {
            if(!Visible) continue;

            var texture = textures[image.TextureIndex];
            int width = texture.Width / frameCounts[image.TextureIndex];
            Rectangle drawFrame2 = new(image.Frame * width, 0, width, texture.Height);

            if(Main.Debug.Enabled)
            {
                NineSlice.DrawNineSlice(
                    Main.GetContent<Texture2D>("Images/Other/tileOutline"),
                    new Rectangle(image.Position.X, image.Position.Y, Width, Height),
                    null,
                    new Point(1),
                    new Point(1),
                    Color.Blue * 0.75f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    ConvertDepth(LayerDepth + 1)
                );
            }

            Renderer.SpriteBatch.Draw(
                texture,
                (image.Position + new Point(this.Width / 2, this.Height / 2)).ToVector2() - HitboxOffset.ToVector2(),
                drawFrame2,
                image.Color * (image.Alpha * 0.5f),
                image.Rotation,
                new Vector2(width / 2, texture.Height / 2),
                image.Scale,
                image.Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                ConvertDepth(LayerDepth + 2)
            );
        }

        if(Main.Debug.Enabled)
        {
            if(Main.Debug.DrawTileCheckingAreas)
            {
                Rectangle newRect = new Rectangle
                {
                    X = Extensions.Floor(this.position.X / (float)World.TileSize),
                    Y = Extensions.Floor(this.position.Y / (float)World.TileSize)
                };
                newRect.Width = MathHelper.Max(1, Extensions.Ceiling(this.Width / (float)World.TileSize) + (Extensions.Floor((this.position.X + (World.TileSize / 2f)) / World.TileSize) - newRect.X));
                newRect.Height = MathHelper.Max(1, Extensions.Ceiling(this.Height / (float)World.TileSize) + (Extensions.Floor((this.position.Y + (World.TileSize / 2f)) / World.TileSize) - newRect.Y));

                for(int x = newRect.X; x < newRect.X + newRect.Width; x++)
                {
                    for(int y = newRect.Y; y < newRect.Y + newRect.Height; y++)
                    {
                        NineSlice.DrawNineSlice(
                            Main.GetContent<Texture2D>("Images/Other/tileOutline"),
                            new Rectangle(x, y, 1, 1).ScalePosition(World.TileSize),
                            null,
                            new Point(1),
                            new Point(1),
                            Color.LimeGreen * 0.75f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            ConvertDepth(LayerDepth + 3)
                        );
                    }
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
                Center.ToVector2() - HitboxOffset.ToVector2(),
                drawFrame,
                Color,
                Rotation,
                new Vector2(width / 2, texture.Height / 2),
                drawScale,
                Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                ConvertedLayerDepth
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
    private int _selectedWeapon;

    public WeaponType[] Weapons { get; } = new WeaponType[2];
    public SpellType[] Spells { get; } = new SpellType[3];

    public WeaponType PrimaryWeapon => Weapons[0];
    public WeaponType SecondaryWeapon => Weapons[1];

    public bool HasWeapon => PrimaryWeapon > WeaponType.Invalid || SecondaryWeapon > WeaponType.Invalid;

    public bool HasSpell => Spells[0] > SpellType.Invalid || Spells[1] > SpellType.Invalid || Spells[2] > SpellType.Invalid;

    public int SelectedWeaponSlot
    {
        get => _selectedWeapon;
        set {
            _selectedWeapon = value % Weapons.Length;
        }
    }

    public WeaponType SelectedWeapon => Weapons[_selectedWeapon];
}
