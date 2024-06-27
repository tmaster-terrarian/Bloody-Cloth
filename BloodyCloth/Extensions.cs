using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth;

public static class Extensions
{
    // public static void DrawStringBold(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int thickness, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
    // {
    //     int _th = MathHelper.Max(thickness, 0);
    //     spriteBatch.DrawString(font, text, position + new Vector2(MathF.Cos(rotation), MathF.Sin(rotation)) * scale * _th, color, rotation, origin, scale, effects, layerDepth, rtl);
    //     spriteBatch.DrawString(font, text, position, color, rotation, origin, scale, effects, layerDepth, rtl);
    // }

    // public static void DrawStringBold(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int thickness, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
    // {
    //     spriteBatch.DrawStringBold(font, text, position, color, thickness, rotation, origin, new Vector2(scale), effects, layerDepth, rtl);
    // }

    // public static void DrawStringBold(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int thickness = 2)
    // {
    //     spriteBatch.DrawStringBold(font, text, position, color, thickness, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
    // }

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
}
