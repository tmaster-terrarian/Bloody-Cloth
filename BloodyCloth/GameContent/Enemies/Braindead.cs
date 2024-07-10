using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Graphics;

namespace BloodyCloth.GameContent.Enemies;

public class Braindead : EnemyDef
{
    public override void OnCreate(Enemy enemy)
    {
        base.OnCreate(enemy);
    }

    public override void Update(Enemy enemy)
    {
        CommonEnemyBehaviors.ApplyFriction(enemy);
        CommonEnemyBehaviors.ApplyGravity(enemy);

        enemy.Pivot = new Vector2(enemy.Texture.Width, enemy.Texture.Height) / 2;
        enemy.TextureVisualOffset = new(enemy.Width / 2, enemy.Height / 2);

        base.Update(enemy);
    }
}
