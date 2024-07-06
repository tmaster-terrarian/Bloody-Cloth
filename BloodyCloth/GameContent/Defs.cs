using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.GameContent;

public static class Defs
{
    static bool isInitialized;

    static readonly Dictionary<ProjectileType, ProjectileDef> projectileDefs = [];
    static readonly Dictionary<WeaponType, WeaponDef> weaponDefs = [];

    public static IReadOnlyDictionary<ProjectileType, ProjectileDef> ProjectileDefs => projectileDefs;

    public static void Initialize()
    {
        if(isInitialized) throw new System.Exception("Game Content has already been initialized!");

        CreateProjectiles();
        CreateWeapons();

        isInitialized = true;
    }

    static void CreateProjectiles()
    {
        projectileDefs.Add(ProjectileType.CrossbowBolt, new Projectiles.FastArrowProjectile {
            Name = "Crossbow Bolt",
            Damage = 1,
            TexturePath = "Projectiles/CrossbowBolt",
            GravityDelay = 10,
        });
    }

    static void CreateWeapons()
    {
        weaponDefs.Add(WeaponType.Crossbow, new WeaponDef {
            Name = "Crossbow",
            IconTexturePath = "UI/Icons/Weapons/Crossbow",
        });
    }
}
