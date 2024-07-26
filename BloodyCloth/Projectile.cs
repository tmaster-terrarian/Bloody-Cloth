using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.GameContent;
using BloodyCloth.Graphics;
using BloodyCloth.Utils;

namespace BloodyCloth;

public class Projectile : MoveableEntity
{
    private static readonly Projectile[] projectiles = new Projectile[200];
    private static uint _projectileID;

    private readonly ProjectileType defId;
    private bool markedForRemoval;

    public float[] GenericFloatValues { get; } = new float[4];
    public int[] GenericIntValues { get; } = new int[4];

    public uint ID { get; private set; }

    public ProjectileType DefID => defId;

    public bool HasValidType => DefID >= 0;

    public int TimeLeft { get; set; } = 180;

    public bool Visible { get; set; } = true;
    public float Alpha { get; set; } = 1;

    public override bool Active => !markedForRemoval;

    public int Damage { get; set; } = 1;

    public int FrameNumber { get; set; } = 1;
    public int Frame { get; set; }

    public string TexturePath { get; set; }

    public Texture2D Texture => Main.LoadContent<Texture2D>("Images/" + TexturePath);

    public Vector2 Pivot { get; set; }
    public Vector2 TextureVisualOffset { get; set; }
    public Vector2 FacingSpecificVisualOffset { get; set; }
    public int FlipY { get; set; } = 1;

    public bool ShouldMove { get; set; } = true;
    public bool DestroyOnCollisionWithWorld { get; set; }

    private Projectile()
    {
        this.ID = _projectileID++;
        this.defId = ProjectileType.Invalid;
        this.CollidesWithJumpthroughs = false;
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
    /// <returns>The index of the new <see cref="Projectile"/>, or <c>-1</c> if there are too many.</returns>
    /// <exception cref="ArgumentException">
    /// </exception>
    public static int Create(ProjectileType type, Point position, Vector2 velocity, int depth = 0)
    {
        if(type <= ProjectileType.Invalid)
        {
            throw new ArgumentException($"{nameof(type)}: Invalid {nameof(EnemyType)} \"{type}\"");
        }

        var projectile = new Projectile(type) {
            position = position,
            velocity = velocity,
            LayerDepth = depth
        };

        for(int i = 0; i < projectiles.Length; i++)
        {
            Projectile proj = projectiles[i];

            if(proj is null || proj.markedForRemoval)
            {
                projectiles[i] = projectile;
                Defs.ProjectileDefs[type].OnCreate(projectile);

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
    /// <returns>The new <see cref="Projectile"/>, or <see langword="null"/> if there are too many..</returns>
    /// <exception cref="ArgumentException">
    /// </exception>
    public static Projectile? CreateDirect(ProjectileType type, Point position, Vector2 velocity, int depth = 0)
    {
        int index = Create(type, position, velocity, depth);
        if(index < 0) return null;

        return projectiles[index];
    }

    public static void Update()
    {
        for(int i = 0; i < projectiles.Length; i++)
        {
            Projectile projectile = projectiles[i];

            if(projectile is null) continue;

            projectile.OnGround = projectile.CheckOnGround();

            if(projectile.HasValidType)
                Defs.ProjectileDefs[projectile.defId].Update(projectile);

            if(projectile.TimeLeft > 0)
                projectile.TimeLeft--;
            if(projectile.TimeLeft == 0)
                projectile.Kill();

            if(projectile.ShouldMove)
            {
                projectile.MoveX(projectile.velocity.X,
                    () => {
                        if(projectile.DestroyOnCollisionWithWorld) projectile.Kill();

                        if(projectile.HasValidType)
                            Defs.ProjectileDefs[projectile.defId].OnCollideX(projectile);
                    }
                );
                projectile.MoveY(projectile.velocity.Y,
                    () => {
                        if(projectile.DestroyOnCollisionWithWorld) projectile.Kill();

                        if(projectile.HasValidType)
                            Defs.ProjectileDefs[projectile.defId].OnCollideY(projectile);
                    }
                );
            }

            if(projectile.markedForRemoval)
            {
                projectiles[i] = null;
            }
        }
    }

    public static void Draw()
    {
        Texture2D tex = null;
        if(Main.Debug.Enabled) tex = Main.LoadContent<Texture2D>("Images/Other/tileOutline");

        for(int i = 0; i < projectiles.Length; i++)
        {
            Projectile projectile = projectiles[i];

            if(projectile is null) continue;

            if(Main.Debug.Enabled)
            {
                if(Main.Debug.DrawTileCheckingAreas && projectile.ShouldMove)
                {
                    Rectangle newRect = new Rectangle
                    {
                        X = MathUtil.FloorToInt(projectile.position.X / (float)World.TileSize),
                        Y = MathUtil.FloorToInt(projectile.position.Y / (float)World.TileSize)
                    };
                    newRect.Width = MathHelper.Max(1, MathUtil.CeilToInt((projectile.position.X + projectile.Width) / (float)World.TileSize) - newRect.X);
                    newRect.Height = MathHelper.Max(1, MathUtil.CeilToInt((projectile.position.Y + projectile.Height) / (float)World.TileSize) - newRect.Y);

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
                                ConvertDepth(projectile.LayerDepth + 1)
                            );
                        }
                    }
                }

                NineSlice.DrawNineSlice(
                    tex,
                    projectile.Hitbox,
                    null,
                    new Point(1),
                    new Point(1),
                    Color.LightBlue * 0.5f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    ConvertDepth(projectile.LayerDepth + 1)
                );
            }

            if(!projectile.Visible) continue;

            if(projectile.HasValidType)
                Defs.ProjectileDefs[projectile.defId].Draw(projectile);
        }
    }

    public void Kill()
    {
        this.markedForRemoval = true;

        if(this.HasValidType)
            Defs.ProjectileDefs[this.defId].OnDestroy(this);
    }

    public static void ClearAll()
    {
        for(int i = 0; i < projectiles.Length; i++)
        {
            projectiles[i] = null;
        }
    }

    public static bool Exists(int index)
    {
        if(index < 0 || index >= projectiles.Length) return false;

        if(projectiles[index] is null || projectiles[index].markedForRemoval) return false;

        return true;
    }

    public static Projectile? GetProjectile(int index)
    {
        if(index < 0 || index >= projectiles.Length) throw new IndexOutOfRangeException(nameof(index));

        if(projectiles[index] is null || projectiles[index].markedForRemoval) return null;

        return projectiles[index];
    }

    public static bool Exists(uint id) => GetProjectile(id) is not null;

    public static Projectile? GetProjectile(uint id)
    {
        if(id < 0) throw new IndexOutOfRangeException(nameof(id));

        for(int i = 0; i < projectiles.Length; i++)
        {
            if(projectiles[i] is null || projectiles[i].markedForRemoval) continue;

            if(projectiles[i].ID == id) return projectiles[i];
        }

        return null;
    }

    public bool CanHurtPlayer(Player player = null)
    {
        player ??= Main.Player;
        return !markedForRemoval && !player.Dead;
    }
}
