using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.GameContent;

public static class Defs
{
    static bool isInitialized;

    public static Dictionary<ProjectileType, ProjectileDef> ProjectileDefs { get; } = [];
    public static Dictionary<WeaponType, WeaponDef> WeaponDefs { get; } = [];
    public static Dictionary<TriggerType, TriggerDef> TriggerDefs { get; } = [];
    public static Dictionary<EnemyType, EnemyDef> EnemyDefs { get; } = [];

    public static void Initialize()
    {
        if(isInitialized) throw new System.Exception("Game Content has already been initialized!");

        CreateProjectiles();
        CreateWeapons();
        CreateTriggers();
        CreateEnemies();

        isInitialized = true;
    }

    private static void CreateProjectiles()
    {
        ProjectileDefs.Add(ProjectileType.CrossbowBolt, new Projectiles.FastArrow {
            Name = "CrossbowBolt",
            Damage = 1,
            TexturePath = "Projectiles/CrossbowBolt",
            GravityDelay = 10,
            Gravity = 0.15f,
        });
    }

    private static void CreateWeapons()
    {
        WeaponDefs.Add(WeaponType.Crossbow, new WeaponDef {
            Name = "Crossbow",
            IconTexturePath = "UI/Icons/Weapons/Crossbow",
        });
    }

    private static void CreateTriggers()
    {
        TriggerDefs.Add(TriggerType.NextRoom, new TriggerDef {
            Name = "NextRoom",
            TriggerOnce = true,
            OnEnter = (Trigger trigger) => {
                Main.NextRoom();
            },
        });
    }

    private static void CreateEnemies()
    {
        EnemyDefs.Add(EnemyType.Dummy, new Enemies.Braindead {
            Name = "Dummy",
            TexturePath = "Enemies/dummy/idle",
            Invincible = true,
            MaxHP = 1000000,
            Size = new(10, 24),
        });
    }
}
