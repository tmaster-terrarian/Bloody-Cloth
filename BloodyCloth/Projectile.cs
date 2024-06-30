using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using BloodyCloth.GameContent;
using Microsoft.Xna.Framework.Graphics;
using BloodyCloth.Utils;
using BloodyCloth.Graphics;
using System.Collections;

namespace BloodyCloth;

public class Projectile : Entity
{
    private static readonly Projectile[] projectiles = new Projectile[200];
    private static uint _projectileID;

    private readonly ProjectileType defId;
    private bool markedForRemoval;
    private float yRemainder;
    private float xRemainder;

    public bool hurtsEnemy;
    public bool hurtsPlayer;
    public bool shouldMove = true;
    public float[] genericFloatValues = new float[4];
    public int[] genericIntValues = new int[4];
    public int frameNumber = 1;
    public int frame;

    public uint ID { get; private set; }

    public int TimeLeft { get; set; } = 180;

    /// <summary>
    /// This will cause insane lag if not used carefully!
    /// </summary>
    public bool MoveExact { get; set; }

    public ProjectileType DefID => defId;

    public bool HasValidType => DefID >= 0;

    public bool Visible { get; set; } = true;
    public int Alpha { get; set; } = 1;

    public override bool Active => !markedForRemoval;

    public int Damage { get; set; } = 1;

    public string TexturePath { get; set; }

    public Texture2D Texture => Main.GetContent<Texture2D>("Images/" + TexturePath);

    public Vector2 Pivot { get; set; }
    public Vector2 TextureVisualOffset { get; set; }
    public Vector2 FacingSpecificVisualOffset { get; set; }
    public int FlipY { get; set; } = 1;

	public bool NudgeOnMove { get; set; } = true;

    public bool DestroyOnCollisionWithWorld { get; set; }

    private Projectile()
    {
        this.ID = _projectileID++;
        this.defId = ProjectileType.Invalid;
        this.CollidesWithJumpthroughs = false;
        this.CollidesWithSolids = false;
    }

    private Projectile(ProjectileType id) : this()
    {
        if(id >= 0)
            this.defId = id;
    }

    /// <summary>
    /// Creates a <see cref="Projectile"/> with some initial values.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    /// <returns>The ID of the new <see cref="Projectile"/>.</returns>
    /// <exception cref="Exception">
    /// When given an invalid <see cref="ProjectileType"/>.
    /// </exception>
    public static int Create(ProjectileType type, Point position, Vector2 velocity)
    {
        if(type <= ProjectileType.Invalid)
        {
            throw new Exception($"Cannot create projectile with invalid {nameof(ProjectileType)} \"{type}\"");
        }

        var projectile = new Projectile(type) {
            position = position,
            velocity = velocity
        };

        Defs.ProjectileDefs[type].OnCreate(projectile);

        for(int i = 0; i < projectiles.Length; i++)
        {
            Projectile proj = projectiles[i];

            if(proj is null || !proj.Active)
            {
                projectiles[i] = projectile;
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Creates a <see cref="Projectile"/> with some initial values.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    /// <returns>The new <see cref="Projectile"/>.</returns>
    /// <exception cref="Exception">
    /// When given an invalid <see cref="ProjectileType"/>.
    /// </exception>
    public static Projectile CreateDirect(ProjectileType type, Point position, Vector2 velocity) => projectiles[Create(type, position, velocity)];

    public static void Update()
    {
        for(int i = 0; i < projectiles.Length; i++)
        {
            Projectile projectile = projectiles[i];

            if(projectile is null) continue;

            if(projectile.markedForRemoval)
            {
                if(projectile.HasValidType)
                    Defs.ProjectileDefs[projectile.defId].OnDestroy(projectile);

                projectiles[i] = null;
                continue;
            }

            projectile.OnGround = projectile.CheckOnGround();

            if(projectile.HasValidType)
                Defs.ProjectileDefs[projectile.defId].Update(projectile);

            if(projectile.TimeLeft > 0)
                projectile.TimeLeft--;
            if(projectile.TimeLeft == 0)
                projectile.Kill();

            projectile.MoveX(projectile.velocity.X,
                () => {
                    projectile.velocity.X = 0;
                }
            );
            projectile.MoveY(projectile.velocity.Y,
                () => {
                    projectile.velocity.Y = 0;
                }
            );
        }
    }

    public static void Draw()
    {
        Texture2D tex = null;
        if(Main.DebugMode) tex = Main.GetContent<Texture2D>("Images/Other/tileOutline");

        for(int i = 0; i < projectiles.Length; i++)
        {
            Projectile projectile = projectiles[i];

            if(projectile is null || projectile.markedForRemoval) continue;

            if(Main.DebugMode)
            {
                // Rectangle newRect = new Rectangle
                // {
                //     X = Extensions.Floor(projectile.position.X * 0.125f),
                //     Y = Extensions.Floor(projectile.position.Y * 0.125f)
                // };
                // newRect.Width = MathHelper.Max(1, Extensions.Ceiling(projectile.Width * 0.125f) + (Extensions.Floor((projectile.position.X + 4) * 0.125f) - newRect.X));
                // newRect.Height = MathHelper.Max(1, Extensions.Ceiling(projectile.Height * 0.125f) + (Extensions.Floor((projectile.position.Y + 4) * 0.125f) - newRect.Y));

                // for(int x = newRect.X; x < newRect.X + newRect.Width; x++)
                // {
                //     for(int y = newRect.Y; y < newRect.Y + newRect.Height; y++)
                //     {
                //         NineSlice.DrawNineSlice(Main.GetContent<Texture2D>("Images/Other/tileOutline"), new Rectangle(x, y, 1, 1).ScalePosition(8), null, new Point(1), new Point(1), Color.LimeGreen * 0.75f);
                //     }
                // }

                NineSlice.DrawNineSlice(tex, projectile.Hitbox, null, new Point(1), new Point(1), Color.LightBlue * 0.5f);
            }

            if(!projectile.Visible) continue;

            if(projectile.HasValidType)
                Defs.ProjectileDefs[projectile.defId].Draw(projectile);
        }
    }

    public void Kill()
    {
        this.markedForRemoval = true;
    }

    private void Move(Action? onCollideX = null, Action? onCollideY = null)
    {
        if(!this.shouldMove) return;

        if(velocity.LengthSquared() < 0.000001f)
            velocity = Vector2.Zero;

        this.MoveX(velocity.X, onCollideX);
        this.MoveY(velocity.Y, onCollideY);
    }

    public void MoveX(float amount, Action? onCollide)
    {
        if(amount == 0) return;

        xRemainder += amount;
        int move = Extensions.Round(xRemainder);
        if(move != 0)
        {
            xRemainder -= move;
            int sign = Math.Sign(move);
            while(move != 0)
            {
                bool col1 = CheckColliding((sign >= 0 ? RightEdge : LeftEdge).Shift(new(sign, 0)));
                if(col1 && !CheckColliding((sign >= 0 ? RightEdge : LeftEdge).Shift(new(sign, -1)), true) && NudgeOnMove)
                {
                    position.X += sign;
                    position.Y -= 1;
                    move = MathUtil.Approach(move, 0, Math.Abs(sign));
                }
                else if(!col1)
                {
                    if(OnGround && NudgeOnMove)
                    {
                        if(!CheckColliding(BottomEdge.Shift(new(sign, 1))) && CheckColliding(BottomEdge.Shift(new(sign, 2))))
                            position.Y += 1;
                    }
                    position.X += sign;
                    move = MathUtil.Approach(move, 0, Math.Abs(sign));
                }
                else
                {
                    if(DestroyOnCollisionWithWorld) Kill();

                    onCollide?.Invoke();
                    break;
                }
            }
        }
    }

    public void MoveY(float amount, Action? onCollide)
    {
        if(amount == 0) return;

        yRemainder += amount;
        int move = Extensions.Round(yRemainder);
        if(move != 0)
        {
            yRemainder -= move;
            int sign = Math.Sign(move);
            while(move != 0)
            {
                if(!CheckColliding((sign >= 0 ? BottomEdge : TopEdge).Shift(new(0, sign)), sign < 0))
                {
                    position.Y += sign;
                    move = MathUtil.Approach(move, 0, Math.Abs(sign));
                    continue;
                }

                if(DestroyOnCollisionWithWorld) Kill();

                onCollide?.Invoke();
                break;
            }
        }
    }
}
