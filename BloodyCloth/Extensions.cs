using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth;

public static class Extensions
{
    public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
    {
        var split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        float x = 0;
        foreach(var word in split)
        {
            spriteBatch.DrawString(font, word, position + (Vector2.UnitX * x), color, rotation, origin, scale, effects, layerDepth, rtl);
            x += (font.MeasureString(word).X + spaceSize) * scale.X;
        }
    }

    public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
    {
        spriteBatch.DrawStringSpacesFix(font, text, position, color, spaceSize, rotation, origin, new Vector2(scale), effects, layerDepth, rtl);
    }

    public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize)
    {
        spriteBatch.DrawStringSpacesFix(font, text, position, color, spaceSize, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
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

    public static int ToInt32(this bool value)
    {
        return value ? 1 : 0;
    }

    public static Point Divide(this Point value, int div)
    {
        return new(value.X / div, value.Y / div);
    }

    public static Point Clamp(this Point value, Point min, Point max)
    {
        return new(MathHelper.Clamp(value.X, min.X, max.X), MathHelper.Clamp(value.Y, min.Y, max.Y));
    }

    public static Point ToPoint(this LDtk.GridPoint gridPoint, int gridSize = 1)
    {
        return new(gridPoint.Cx * gridSize, gridPoint.Cy * gridSize);
    }

    public static float ToRotation(this Vector2 value)
    {
        return (float)Math.Atan2(value.Y, value.X);
    }
}
