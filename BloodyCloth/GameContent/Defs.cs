using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.GameContent;

public static class Defs
{
    private static bool isInitialized;

    private static readonly Dictionary<ProjectileType, ProjectileDef> projectileDefs = [];

    public static IReadOnlyDictionary<ProjectileType, ProjectileDef> ProjectileDefs => projectileDefs;

    public static void Initialize()
    {
        if(isInitialized) throw new System.Exception("Content have already been initialized!");

        CreateProjectiles();
        CreateWeaponItems();

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

    static void CreateWeaponItems()
    {
        
    }
}
