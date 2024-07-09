using Microsoft.Xna.Framework;

using BloodyCloth.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.GameContent.Projectiles;

public class FastArrow : ProjectileDef
{
    public float Gravity { get; set; } = 0.2f;
    public int GravityDelay { get; set; } = 30;

    public override void OnCreate(Projectile projectile)
    {
        projectile.DestroyOnCollisionWithWorld = true;
        projectile.CollidesWithJumpthroughs = false;
        projectile.CollidesWithSolids = true;

        base.OnCreate(projectile);
    }

    public override void Update(Projectile projectile)
    {
        ref int gravityTicker = ref projectile.genericIntValues[0];

        if(gravityTicker >= 0 && gravityTicker < GravityDelay)
            gravityTicker++;

        if(gravityTicker >= GravityDelay)
        {
            projectile.velocity.Y = MathUtil.Approach(projectile.velocity.Y, 20, Gravity);
            projectile.velocity.X *= 0.985f;
        }

        projectile.Rotation = projectile.velocity.ToRotation();

        projectile.FlipY = projectile.velocity.X < 0 ? -1 : 1;

        projectile.Pivot = new Vector2(projectile.Texture.Width, projectile.Texture.Height) / 2 + new Vector2(projectile.Width / 2, 0);
        projectile.TextureVisualOffset = new(projectile.Width / 2, projectile.Height / 2);

        base.Update(projectile);
    }
}
