using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Graphics;

namespace BloodyCloth.GameContent.Enemies;

public class Braindead : EnemyDef
{
    public string TexturePath { get; set; }

    public override void Update(Enemy enemy)
    {
        // TODO: add gravity
    }

    public override void Draw(Enemy enemy)
    {
        var tex = Main.GetContent<Texture2D>("Images/" + TexturePath);
        Renderer.SpriteBatch.Draw(tex, enemy.Center.ToVector2(), null, Color.White, enemy.Rotation, new Vector2(tex.Width / 2f, tex.Height / 2f), enemy.DrawScale, SpriteEffects.None, enemy.ConvertedLayerDepth);
    }
}
