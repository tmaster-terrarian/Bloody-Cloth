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
    /// <returns>The ID of the new <see cref="Projectile"/>.</returns>
    /// <exception cref="Exception">
    /// When given an invalid <see cref="ProjectileType"/>.
    /// </exception>
    public static int Create(ProjectileType type, Point position, Vector2 velocity, int depth = 0)
    {
        if(type <= ProjectileType.Invalid)
        {
            throw new Exception($"Cannot create projectile with invalid {nameof(ProjectileType)} \"{type}\"");
        }

        var projectile = new Projectile(type) {
            position = position,
            velocity = velocity,
            LayerDepth = depth
        };

        Defs.ProjectileDefs[type].OnCreate(projectile);

        for(int i = 0; i < projectiles.Length; i++)
        {
            Projectile proj = projectiles[i];

            if(proj is null || proj.markedForRemoval)
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
    public static Projectile CreateDirect(ProjectileType type, Point position, Vector2 velocity, int depth = 0) => projectiles[Create(type, position, velocity, depth)];

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

            if(projectile.shouldMove)
            {
                projectile.MoveX(projectile.velocity.X,
                    () => {
                        if(projectile.DestroyOnCollisionWithWorld) projectile.Kill();

                        projectile.velocity.X = 0;
                    }
                );
                projectile.MoveY(projectile.velocity.Y,
                    () => {
                        if(projectile.DestroyOnCollisionWithWorld) projectile.Kill();

                        projectile.velocity.Y = 0;
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
        if(Main.Debug.Enabled) tex = Main.GetContent<Texture2D>("Images/Other/tileOutline");

        for(int i = 0; i < projectiles.Length; i++)
        {
            Projectile projectile = projectiles[i];

            if(projectile is null) continue;

            if(Main.Debug.Enabled)
            {
                if(Main.Debug.DrawTileCheckingAreas)
                {
                    Rectangle newRect = new Rectangle
                    {
                        X = Extensions.Floor(projectile.position.X / (float)World.TileSize),
                        Y = Extensions.Floor(projectile.position.Y / (float)World.TileSize)
                    };
                    newRect.Width = MathHelper.Max(1, Extensions.Ceiling((projectile.position.X + projectile.Width) / (float)World.TileSize) - newRect.X);
                    newRect.Height = MathHelper.Max(1, Extensions.Ceiling((projectile.position.Y + projectile.Height) / (float)World.TileSize) - newRect.Y);

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

            if(!projectile.Visible || projectile.markedForRemoval) continue;

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

    public static void ClearProjectiles()
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
}
