using System;

using Microsoft.Xna.Framework;

using BloodyCloth.Utils;

namespace BloodyCloth.GameContent.Enemies;

public static class CommonEnemyBehaviors
{
    public static void ApplyGravity(Enemy enemy, float gravity = 0.2f, float cap = 20f)
    {
        if(!enemy.OnGround && gravity != 0)
        {
            enemy.velocity.Y = MathUtil.Approach(enemy.velocity.Y, Math.Abs(cap) * Math.Sign(gravity), Math.Abs(gravity));
        }
    }

    public static void ApplyFriction(Enemy enemy, float fric = 0.2f)
    {
        if(enemy.OnGround && fric != 0)
        {
            enemy.velocity.X = MathUtil.Approach(enemy.velocity.X, 0, Math.Abs(fric));
        }
    }
}
