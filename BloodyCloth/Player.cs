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
    float stateTimer;

    readonly float gravity = 0.2f;
    readonly float baseMoveSpeed = 3f;
    readonly float baseJumpSpeed = -4.5f;
    readonly float baseGroundAcceleration = 0.15f;
    readonly float baseGroundFriction = 0.1f;
    readonly float baseAirAcceleration = 0.07f;
    readonly float baseAirFriction = 0.05f;

    Vector2 renderTargetOffset = Main.Camera.Position;

    float moveSpeed;
    float jumpSpeed;
    float accel;
    float fric;

    bool useGravity = true;
    bool jumpCancelled;
    bool running;

    Point hitboxOffset = new(0, 5);
    readonly RenderTarget2D renderTarget;

    bool fxTrail;
    int fxTrailCounter;
    readonly List<AfterImage> afterImages = [];

    int bonusHp = 0;
    int shieldIFrames = 0;

    readonly PlayerInputMapping inputMapping = new PlayerInputMapping {
        // rebind here with Name = MappedInput
    };

    public PlayerState State {
        get => _state;
        set {
            if(value < 0 || value > PlayerState.Dead) value = PlayerState.IgnoreState;

            if(_state != value)
            {
                _stateJustChanged = true;
                stateTimer = 0;

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

    public bool Dead { get; private set; }

    int testWeaponCooldown = 0;

    readonly List<Texture2D> textures = [];
    readonly List<int> frameCounts = [];
    int textureIndex;
    float frame;

    Random nugdeRandom = new();

    enum TextureIndex
    {
        Idle,
        Running
    }

    public Player()
    {
        Width = 12;
        Height = 38;

        Bottom = new(100, 104);

        LayerDepth = 50;

        string texPath = "Images/Player/";
        void addTex(string path, int count = 1) => AddTexture(Main.LoadContent<Texture2D>(texPath + path), count);

        addTex("idle");
        addTex("run", 6);

        renderTarget = new(Renderer.GraphicsDevice, Renderer.ScreenSize.X, Renderer.ScreenSize.Y);
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
                if(OnGround)
                {
                    textureIndex = (int)TextureIndex.Idle;
                    frame = 0;
                }
                else
                {
                    textureIndex = (int)TextureIndex.Idle; // jump texture, replace later pls
                }
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
        int inputDir = InputMapping.Right.IsDown.ToInt32() - InputMapping.Left.IsDown.ToInt32();

        bool wasOnGround = OnGround;
        bool onJumpthrough = CheckCollidingJumpthrough(BottomEdge.Shift(0, 1));
        if(onJumpthrough) OnGround = true;
        else OnGround = CheckColliding(BottomEdge.Shift(0, 1));

        bool getPushedByEnemies = false;

        if(!wasOnGround && OnGround)
        {
            jumpCancelled = false;
        }

        RecalculateStats();

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
                    textureIndex = (int)TextureIndex.Idle;
                }

                break;
            case PlayerState.Normal:
                useGravity = true;
                getPushedByEnemies = true;

                if(inputDir != 0)
                {
                    Facing = inputDir;

                    if(OnGround)
                    {
                        running = true;

                        if(velocity.Y >= 0)
                        {
                            textureIndex = (int)TextureIndex.Running;
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

                    if(OnGround && Math.Abs(velocity.X) < 1.5f)
                    {
                        textureIndex = (int)TextureIndex.Idle;
                    }
                }

                if(!OnGround)
                {
                    if(InputMapping.Down.Released && velocity.Y < 0 && !jumpCancelled)
                    {
                        jumpCancelled = true;
                        velocity.Y /= 2;
                    }
                }
                else
                {
                    if(onJumpthrough && InputMapping.Down.IsDown && !CheckColliding(BottomEdge.Shift(new(0, 2)), true))
                    {
                        position.Y += 2;

                        onJumpthrough = CheckCollidingJumpthrough(BottomEdge.Shift(0, 1));
                        if(onJumpthrough) OnGround = true;
                        else OnGround = CheckColliding(BottomEdge.Shift(0, 1));

                        getPushedByEnemies = false;
                    }
                }

                if(running)
                {
                    frame += Math.Abs(velocity.X) / frameCounts[textureIndex] / 2.5f;
                }

                fxTrail = Math.Abs(velocity.X) > 1f * moveSpeed;

                // ...

                if(OnGround && InputMapping.Jump.Pressed)
                {
                    velocity.Y = jumpSpeed;
                }

                break;
            case PlayerState.IgnoreState: default:
                break;
        }

        if(!Dead && shieldIFrames > 0)
            shieldIFrames--;

        if(!OnGround && useGravity)
        {
            if(velocity.Y >= 0.1)
                velocity.Y = MathUtil.Approach(velocity.Y, 20, gravity);
            if(velocity.Y < 0)
                velocity.Y = MathUtil.Approach(velocity.Y, 20, gravity);
            else if (velocity.Y < 2)
                velocity.Y = MathUtil.Approach(velocity.Y, 20, gravity * 0.25f);
        }

        if(getPushedByEnemies)
        {
            var e = Enemy.EnemyPlace(Hitbox);
            if(e is not null && e.PushesPlayer)
            {
                int diffX = Center.X - e.Center.X;
                diffX = diffX != 0 ? diffX : (nugdeRandom.Next(3) - 1);

                if(e.PushesPlayer || e.PushedByPlayer)
                {
                    velocity.X += 0.08f * diffX * MathHelper.Max(1, e.Mass - 10);
                    MoveX(Math.Sign(diffX), null);

                    if(Bottom.Y <= e.Center.Y && velocity.Y >= 0)
                    {
                        velocity.Y -= 0.51f * MathHelper.Max(1, e.Mass - 10);
                    }
                }

                if(e.PushedByPlayer)
                {
                    e.velocity.X += -0.08f * diffX * (MathHelper.Max(1, e.Mass - 10) / 4);
                    e.MoveX(-Math.Sign(diffX), null);

                    if(e.Bottom.Y <= Center.Y && e.velocity.Y >= 0)
                    {
                        e.velocity.Y -= 0.51f * (MathHelper.Max(1, e.Mass - 10) / 4);
                    }
                }
            }
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
            getPushedByEnemies = false;
        }

        MoveX(velocity.X, () => {
            velocity.X = 0;
        });
        MoveY(velocity.Y, () => {
            if(!(InputMapping.Down.IsDown && CheckCollidingJumpthrough(BottomEdge.Shift(new(0, 1)))))
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

        #region pre-draw setup

        renderTargetOffset = Main.Camera.Position;

        Renderer.GraphicsDevice.SetRenderTarget(renderTarget);
        Renderer.GraphicsDevice.Clear(Color.Transparent);
        Renderer.SpriteBatch.Base.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap);

        var texture = textures[textureIndex];
        int width = texture.Width / frameCounts[textureIndex];
        Rectangle drawFrame = new((int)frame * width, 0, width, texture.Height);

        Renderer.SpriteBatch.Base.Draw(
            texture,
            Center.ToVector2() - hitboxOffset.ToVector2() - renderTargetOffset,
            drawFrame,
            Color,
            Rotation,
            new Vector2(width / 2, texture.Height / 2),
            drawScale,
            Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            ConvertedLayerDepth
        );

        Renderer.SpriteBatch.Base.End();
        Renderer.GraphicsDevice.SetRenderTarget(null);

        #endregion
    }

    private void RecalculateStats()
    {
        moveSpeed = baseMoveSpeed;
        jumpSpeed = baseJumpSpeed;

        accel = baseGroundAcceleration;
        fric = baseGroundFriction;
        if(!OnGround)
        {
            accel = baseAirAcceleration;
            fric = baseAirFriction;
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
            Rectangle drawFrame = new(image.Frame * width, 0, width, texture.Height);

            if(Main.Debug.Enabled)
            {
                NineSlice.DrawNineSlice(
                    Main.LoadContent<Texture2D>("Images/Other/tileOutline"),
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
                (image.Position + new Point(this.Width / 2, this.Height / 2)).ToVector2() - hitboxOffset.ToVector2(),
                drawFrame,
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
                    X = this.position.X / World.TileSize,
                    Y = this.position.Y / World.TileSize
                };
                newRect.Width = MathHelper.Max(1, MathUtil.CeilToInt((this.position.X + this.Width) / (float)World.TileSize) - newRect.X);
                newRect.Height = MathHelper.Max(1, MathUtil.CeilToInt((this.position.Y + this.Height) / (float)World.TileSize) - newRect.Y);

                for(int x = newRect.X; x < newRect.X + newRect.Width; x++)
                {
                    for(int y = newRect.Y; y < newRect.Y + newRect.Height; y++)
                    {
                        NineSlice.DrawNineSlice(
                            Main.LoadContent<Texture2D>("Images/Other/tileOutline"),
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
            Renderer.SpriteBatch.Draw(renderTarget, renderTargetOffset, Color.White);
        }

        if(Main.Debug.Enabled)
        {
            NineSlice.DrawNineSlice(Main.LoadContent<Texture2D>("Images/Other/tileOutline"), Hitbox, null, new Point(1), new Point(1), Color.Red * 0.5f);
        }
    }

    public void Hurt()
    {
        if(shieldIFrames > 0 || Dead) return;

        if(bonusHp > 0)
        {
            bonusHp--;
            if(bonusHp == 0)
            {
                shieldIFrames = 6;
            }
        }
        else
        {
            Dead = true;
            State = PlayerState.Dead;
        }
    }

    public bool IsRiding(Ecs.Components.Solid solid)
    {
        return solid.Collidable && solid.Intersects(Hitbox.Shift(new Point(0, 1)));
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
