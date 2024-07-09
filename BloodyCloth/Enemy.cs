using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.GameContent;
using BloodyCloth.Graphics;
using BloodyCloth.Utils;
using System.Collections.Generic;

namespace BloodyCloth;

public enum EnemyState
{
    IgnoreState,
    Normal,
    Dead
}

public class Enemy : MoveableEntity
{
    private static readonly Enemy[] enemies = new Enemy[1000];
    private static uint _enemyID;

    private EnemyType defId = EnemyType.Invalid;
    private bool markedForRemoval;
    private EnemyState _state;

    private float hp = 1;
    private int regenCounter;
    private int regenTime = 60;

    public uint ID { get; private set; }

    public EnemyType DefID => defId;

    public bool HasValidType => DefID >= 0;

    public bool Visible { get; set; } = true;

    public float HP { get => hp; private set => hp = value; }

    public int MaxHP { get; set; } = 1;

    public bool RegeneratesHP { get; set; }

    public bool Invincible { get; set; }

    public bool IsDead { get; private set; }

    public bool ShouldMove { get; set; } = true;

    public float StateTimer { get; set; }
    public EnemyState State
    {
        get => _state;
        set {
            if(_state != value)
            {
                StateTimer = 0;

                if(HasValidType)
                {
                    Defs.EnemyDefs[defId].OnStateExit(this, _state);
                    Defs.EnemyDefs[defId].OnStateEnter(this, value);
                }

                _state = value;
            }
        }
    }

    private Enemy()
    {
        this.ID = _enemyID++;
        this.defId = EnemyType.Invalid;
        this.State = EnemyState.Normal;
    }

    private Enemy(EnemyType id) : this()
    {
        if(id >= 0)
            this.defId = id;
    }

    /// <summary>
    /// Creates a <see cref="Enemy"/> with some initial values.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    /// <returns>The index of the new <see cref="Enemy"/>, or <c>-1</c> if there are too many.</returns>
    /// <exception cref="ArgumentException">
    /// </exception>
    public static int Create(EnemyType type, Point position, Vector2 velocity, int depth = 0)
    {
        if(type <= EnemyType.Invalid)
        {
            throw new ArgumentException($"{nameof(type)}: Invalid {nameof(EnemyType)} \"{type}\"");
        }

        var enemy = new Enemy(type) {
            position = position,
            velocity = velocity,
            LayerDepth = depth
        };

        for(int i = 0; i < enemies.Length; i++)
        {
            Enemy e = enemies[i];

            if(e is null || e.markedForRemoval)
            {
                Defs.EnemyDefs[type].OnCreate(enemy);
                enemy.Init();

                enemies[i] = enemy;

                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Creates a <see cref="Enemy"/> with some initial values.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    /// <returns>The new <see cref="Enemy"/>, or <c><see langword="null"/></c> if there are too many.</returns>
    /// <exception cref="ArgumentException">
    /// </exception>
    public static Enemy? CreateDirect(EnemyType type, Point position, Vector2 velocity, int depth = 0)
    {
        int index = Create(type, position, velocity, depth);
        if(index < 0) return null;

        return enemies[index];
    }

    private void Init()
    {
        HP = MaxHP;
    }

    public static void Update()
    {
        for(int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];

            if(enemy is null) continue;

            enemy.OnGround = enemy.CheckOnGround();

            if(enemy.HasValidType)
                Defs.EnemyDefs[enemy.defId].Update(enemy);

            if(enemy.HP > enemy.MaxHP)
                enemy.HP = enemy.MaxHP;
            if(enemy.HP <= 0 && !enemy.Invincible)
                enemy.Kill();

            if(!enemy.IsDead)
            {
                if(enemy.RegeneratesHP)
                {
                    if(++enemy.regenCounter >= enemy.regenTime)
                    {
                        enemy.regenCounter = 0;
                        enemy.HP++;
                    }
                }
            }

            if(enemy.ShouldMove)
            {
                enemy.MoveX(enemy.velocity.X,
                    () => {
                        enemy.velocity.X = 0;
                    }
                );
                enemy.MoveY(enemy.velocity.Y,
                    () => {
                        enemy.velocity.Y = 0;
                    }
                );
            }

            if(enemy.markedForRemoval)
            {
                enemies[i] = null;
            }
        }
    }

    public static void Draw()
    {
        Texture2D tex = null;
        if(Main.Debug.Enabled) tex = Main.LoadContent<Texture2D>("Images/Other/tileOutline");

        for(int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];

            if(enemy is null) continue;

            if(Main.Debug.Enabled)
            {
                if(Main.Debug.DrawTileCheckingAreas && enemy.ShouldMove)
                {
                    Rectangle newRect = new Rectangle
                    {
                        X = Extensions.FloorToInt(enemy.position.X / (float)World.TileSize),
                        Y = Extensions.FloorToInt(enemy.position.Y / (float)World.TileSize)
                    };
                    newRect.Width = MathHelper.Max(1, Extensions.CeilToInt((enemy.position.X + enemy.Width) / (float)World.TileSize) - newRect.X);
                    newRect.Height = MathHelper.Max(1, Extensions.CeilToInt((enemy.position.Y + enemy.Height) / (float)World.TileSize) - newRect.Y);

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
                                ConvertDepth(enemy.LayerDepth + 1)
                            );
                        }
                    }
                }

                NineSlice.DrawNineSlice(
                    tex,
                    enemy.Hitbox,
                    null,
                    new Point(1),
                    new Point(1),
                    Color.LightBlue * 0.5f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    ConvertDepth(enemy.LayerDepth + 1)
                );
            }

            if(!enemy.Visible) continue;

            if(enemy.HasValidType)
                Defs.EnemyDefs[enemy.defId].Draw(enemy);
        }
    }

    /// <summary>
    /// Attempt to harm the <see cref="Enemy"/> instance.
    /// </summary>
    /// <param name="damage">The amount of damage to hit the <see cref="Enemy"/> with.</param>
    /// <returns>The actual amount of damage taken.</returns>
    public float Hurt(float damage)
    {
        damage = MathHelper.Max(0, damage);
        if(damage == 0) return damage;

        // damage reduction logic here

        if(this.Invincible) return damage;

        HP -= damage;

        return damage;
    }

    public void Kill()
    {
        IsDead = true;
        if(!markedForRemoval)
        {
            if(HasValidType) Defs.EnemyDefs[DefID].OnDestroy(this);
            else Remove();
        }
    }

    void Remove()
    {
        markedForRemoval = true;
    }

    public static Enemy? EnemyPlace(Rectangle bbox)
    {
        foreach(var enemy in enemies)
        {
            if(enemy is null || !enemy.Active || enemy.markedForRemoval) continue;

            if(Vector2.DistanceSquared(bbox.Center.ToVector2(), enemy.Hitbox.Center.ToVector2()) > 1024) continue;

            if(bbox.Right <= enemy.Hitbox.Left) continue;
            if(bbox.Bottom <= enemy.Hitbox.Top) continue;
            if(bbox.Left >= enemy.Hitbox.Right) continue;
            if(bbox.Top >= enemy.Hitbox.Bottom) continue;

            Main.World.NumCollisionChecks++;
            if(enemy.Hitbox.Intersects(bbox)) return enemy;
        }
        return null;
    }

    public static List<Enemy> GetIntersectingEnemies(Rectangle bbox)
    {
        List<Enemy> list = [];
        foreach(var enemy in enemies)
        {
            if(enemy is null || !enemy.Active || enemy.markedForRemoval) continue;

            if(Vector2.DistanceSquared(bbox.Center.ToVector2(), enemy.Hitbox.Center.ToVector2()) > 1024) continue;

            if(bbox.Right <= enemy.Hitbox.Left) continue;
            if(bbox.Bottom <= enemy.Hitbox.Top) continue;
            if(bbox.Left >= enemy.Hitbox.Right) continue;
            if(bbox.Top >= enemy.Hitbox.Bottom) continue;

            Main.World.NumCollisionChecks++;
            if(enemy.Hitbox.Intersects(bbox)) list.Add(enemy);
        }
        return list;
    }

    public static bool EnemyMeeting(Rectangle bbox) => EnemyPlace(bbox) is not null;
}
