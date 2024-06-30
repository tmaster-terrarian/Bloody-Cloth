using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Graphics;

namespace BloodyCloth.GameContent;

public class ProjectileDef : ContentDef, ITexturedContentDef
{
    public string TexturePath { get; set; }

    public virtual void OnCreate(Projectile projectile)
    {
        projectile.TexturePath = TexturePath;
    }

    public virtual void Update(Projectile projectile) {}

    public virtual void Draw(Projectile projectile)
    {
        int width = projectile.Texture.Width / projectile.frameNumber;
        Rectangle srcRect = new(projectile.frame * width, 0, width, projectile.Texture.Height);

        Renderer.SpriteBatch.Draw(
            projectile.Texture,
            projectile.TopLeft.ToVector2() + projectile.TextureVisualOffset + new Vector2(projectile.FacingSpecificVisualOffset.X * projectile.Facing, projectile.FacingSpecificVisualOffset.Y * projectile.FlipY),
            srcRect,
            projectile.Color * projectile.Alpha,
            projectile.Rotation,
            projectile.Pivot,
            projectile.DrawScale,
            (projectile.Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (projectile.FlipY < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None),
            0
        );
    }

    public virtual void OnDestroy(Projectile projectile) {}
}

public enum ProjectileType
{
    Invalid = -1,
    CrossbowBolt,
}
