using Microsoft.Xna.Framework;

using BloodyCloth.Utils;
using Microsoft.Xna.Framework.Graphics;
using BloodyCloth.Graphics;

namespace BloodyCloth.GameContent.Projectiles;

public class ShittyParticle : ProjectileDef
{
    public float Gravity { get; set; } = 0.2f;

    public int MaxBounces { get; set; } = 4;

    private static System.Random random = new();

    public override void OnCreate(Projectile projectile)
    {
        projectile.DestroyOnCollisionWithWorld = false;
        projectile.CollidesWithJumpthroughs = false;
        projectile.CollidesWithSolids = false;

        projectile.GenericIntValues[2] = 180;

        projectile.velocity = new Vector2(random.NextSingle() * 2 - 1, random.NextSingle() * 2 - 1) * 3;

        var screenPos = projectile.TopLeft.ToVector2() - Main.Camera.Position;
        Color[] col = new Color[1];

        Renderer.RenderTarget.GetData(0, new(screenPos.ToPoint(), new(1)), col, 0, 1);
        projectile.Color = col[0];

        base.OnCreate(projectile);
    }

    public override void Update(Projectile projectile)
    {
        ref int landed = ref projectile.GenericIntValues[1];
        ref int fadeDelay = ref projectile.GenericIntValues[2];

        projectile.velocity.Y = MathUtil.Approach(projectile.velocity.Y, 20, Gravity);

        base.Update(projectile);

        if(landed >= 1)
        {
            fadeDelay--;
            if(fadeDelay <= 0)
            {
                if(projectile.Alpha > 0)
                    projectile.Alpha = MathUtil.Approach(projectile.Alpha, 0, 0.1f);
                if(projectile.Alpha == 0)
                {
                    projectile.Kill();
                }
            }
        }
    }

    public override void OnCollideX(Projectile projectile)
    {
        if(projectile.GenericIntValues[0] < MaxBounces)
        {
            projectile.GenericIntValues[0]++;
            projectile.velocity.X = -projectile.velocity.X * 0.8f;
        }
        else
        {
            projectile.GenericIntValues[1] = 1;
        }
    }

    public override void OnCollideY(Projectile projectile)
    {
        if(projectile.GenericIntValues[0] < MaxBounces)
        {
            projectile.GenericIntValues[0]++;
            projectile.velocity.Y = -projectile.velocity.Y * 0.75f;
            projectile.velocity.X = -projectile.velocity.X * 0.6f * ((random.Next(2) < 1) ? -1 : 1);
        }
        else
        {
            projectile.GenericIntValues[1] = 1;
        }
    }
}
