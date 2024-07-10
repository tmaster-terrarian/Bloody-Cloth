using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Graphics;

namespace BloodyCloth.GameContent;

public abstract class EnemyDef : AbstractDef
{
    public string TexturePath { get; set; }
    public Point Size { get; set; } = new(8);

    public int MaxHP { get; set; } = 1;
    public bool Invincible { get; set; }
    public bool RegeneratesHP { get; set; }
    public int RegenHealthPerSecond { get; set; } = 1;

    public bool PushesPlayer { get; set; } = true;
    public bool PushedByPlayer { get; set; } = true;
    public float Mass { get; set; } = 10;

    public virtual void OnCreate(Enemy enemy)
    {
        enemy.TexturePath = TexturePath;
        enemy.Width = Size.X;
        enemy.Height = Size.Y;

        enemy.MaxHP = MaxHP;
        enemy.Invincible = Invincible;
        enemy.RegeneratesHP = RegeneratesHP;
        enemy.RegenHealthPerSecond = RegenHealthPerSecond;

        enemy.PushesPlayer = PushesPlayer;
        enemy.PushedByPlayer = PushedByPlayer;
        enemy.Mass = Mass;
    }

    public virtual void OnStateEnter(Enemy enemy, EnemyState state) {}

    public virtual void OnStateExit(Enemy enemy, EnemyState state) {}

    public virtual void Update(Enemy enemy) {}

    public virtual void Draw(Enemy enemy)
    {
        int width = enemy.Texture.Width / enemy.FrameNumber;
        Rectangle srcRect = new(enemy.Frame * width, 0, width, enemy.Texture.Height);

        Renderer.SpriteBatch.Draw(
            enemy.Texture,
            enemy.TopLeft.ToVector2() + enemy.TextureVisualOffset + new Vector2(enemy.FacingSpecificVisualOffset.X * enemy.Facing, enemy.FacingSpecificVisualOffset.Y * enemy.FlipY),
            srcRect,
            enemy.Color * enemy.Alpha,
            enemy.Rotation,
            enemy.Pivot,
            enemy.DrawScale,
            (enemy.Facing < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (enemy.FlipY < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None),
            enemy.ConvertedLayerDepth
        );
    }

    public virtual void OnDestroy(Enemy enemy) {}
}

public enum EnemyType
{
    Invalid = -1,
    Dummy
}
