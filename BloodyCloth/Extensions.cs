using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth;

public static class Extensions
{
    public static void DrawStringBold(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int thickness, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
    {
        int _th = MathHelper.Max(thickness, 0);
        spriteBatch.DrawString(font, text, position + new Vector2(MathF.Cos(rotation), MathF.Sin(rotation)) * scale * _th, color, rotation, origin, scale, effects, layerDepth, rtl);
        spriteBatch.DrawString(font, text, position, color, rotation, origin, scale, effects, layerDepth, rtl);
    }

    public static void DrawStringBold(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int thickness, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
    {
        spriteBatch.DrawStringBold(font, text, position, color, thickness, rotation, origin, new Vector2(scale), effects, layerDepth, rtl);
    }

    public static void DrawStringBold(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int thickness = 2)
    {
        spriteBatch.DrawStringBold(font, text, position, color, 0, thickness, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
    }

    public static int Round(float value)
    {
        return (int)Math.Round(value);
    }

    public static int Ceiling(float value)
    {
        return (int)Math.Ceiling(value);
    }

    public static int Floor(float value)
    {
        return (int)Math.Floor(value);
    }
}
