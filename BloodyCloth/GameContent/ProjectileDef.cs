using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Graphics;
using BloodyCloth.Utils;

namespace BloodyCloth.GameContent;

public class ProjectileDef : AbstractDef, IDealsDamageContentDef
{
    public string TexturePath { get; set; }
    public Point Size { get; set; } = new(8);

    public int Damage { get; set; } = 1;
    public bool CanHurtPlayer { get; set; }
    public bool CanHurtEnemy { get; set; } = true;

    public virtual void OnCreate(Projectile projectile)
    {
        projectile.TexturePath = TexturePath;
        projectile.Width = Size.X;
        projectile.Height = Size.Y;

        projectile.Damage = Damage;
    }

    public virtual void Update(Projectile projectile)
    {
        if(CanHurtEnemy)
        {
            Enemy enemy = Enemy.EnemyPlace(projectile.Hitbox.Shift(projectile.velocity.ToPoint()));
            if(enemy is not null)
            {
                // contact!
                OnHitAnything(projectile);
                OnHitEnemy(projectile, enemy);
            }
        }

        if(CanHurtPlayer)
        {
            if(Main.Player.Hitbox.Intersects(projectile.Hitbox) && projectile.CanHurtPlayer())
            {
                OnHitAnything(projectile);
                OnHitPlayer(projectile, Main.Player);
            }
        }
    }

    public virtual void Draw(Projectile projectile)
    {
        int width = projectile.Texture.Width / projectile.FrameNumber;
        Rectangle srcRect = new(projectile.Frame * width, 0, width, projectile.Texture.Height);

        Renderer.SpriteBatch.Draw(
            projectile.Texture,
            projectile.TopLeft.ToVector2() + projectile.TextureVisualOffset + new Vector2(projectile.FacingSpecificVisualOffset.X * projectile.Facing, projectile.FacingSpecificVisualOffset.Y * projectile.FlipY),
            srcRect,
            projectile.Color * projectile.Alpha,
            projectile.Rotation,
            projectile.Pivot,
            projectile.DrawScale,
            (projectile.Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (projectile.FlipY < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None),
            projectile.ConvertedLayerDepth
        );
    }

    public virtual void OnHitAnything(Projectile projectile) {}

    public virtual void OnHitEnemy(Projectile projectile, Enemy enemy)
    {
        float dmg = enemy.Hurt(projectile.Damage);
        Main.LastPlayerHitDamage = MathUtil.CeilToInt(dmg);

        projectile.Kill();
    }

    public virtual void OnHitPlayer(Projectile projectile, Player player)
    {
        // player damage logic
        projectile.Kill();
    }

    public virtual void OnDestroy(Projectile projectile) {}
}

public enum ProjectileType
{
    Invalid = -1,
    CrossbowBolt,
}
