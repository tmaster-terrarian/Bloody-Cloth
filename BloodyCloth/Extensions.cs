using System;
using BloodyCloth.Utils;
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

    public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize, float rotation, Vector2 origin, float scale = 1, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
    {
        spriteBatch.DrawStringSpacesFix(font, text, position, color, spaceSize, rotation, origin, new Vector2(scale), effects, layerDepth, rtl);
    }

    public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize)
    {
        spriteBatch.DrawStringSpacesFix(font, text, position, color, spaceSize, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
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

    public static Color Multiply(this Color value, float scale)
    {
        return Color.Multiply(value, scale);
    }

    public static Color Divide(this Color value, float scale)
    {
        return Color.Multiply(value, 1f/scale);
    }

    public static Color Multiply(this Color color, Color value)
    {
        Vector4 dest = new(color.ToVector3() / (color.A / 255f), color.A / 255f);
        Vector4 src = new(value.ToVector3() / (value.A / 255f), value.A / 255f);

        return new Color((src * dest) + (dest * 0f));
    }

    public static Color Divide(this Color color, Color value)
    {
        float destA = color.ToVector4().W;
        float srcA = value.ToVector4().W;
        Vector3 dest = color.ToVector3() / destA;
        Vector3 src = value.ToVector3() / srcA;

        return new Color((src / dest) + (dest * 0f));
    }

    public static Color Add(this Color color, Color value)
    {
        float destA = color.ToVector4().W;
        float srcA = value.ToVector4().W;
        Vector4 dest = new(color.ToVector3() / destA, destA);
        Vector4 src = new(value.ToVector3() / srcA, srcA);
        if(color.A == 0)
        {
            dest = Vector4.Zero;
        }
        if(value.A == 0)
        {
            src = Vector4.Zero;
        }

        return Color.FromNonPremultiplied(dest + (src * srcA));
    }

    public static Color Subtract(this Color color, Color value)
    {
        float destA = color.ToVector4().W;
        float srcA = value.ToVector4().W;
        Vector4 dest = new(color.ToVector3() / destA, destA);
        Vector4 src = new(value.ToVector3() / srcA, srcA);
        if(color.A == 0)
        {
            dest = Vector4.Zero;
        }
        if(value.A == 0)
        {
            src = Vector4.Zero;
        }

        return Color.FromNonPremultiplied(dest - (src * srcA));
    }

    public static Color AddPreserveAlpha(this Color color, Color value)
    {
        float destA = color.ToVector4().W;
        float srcA = value.ToVector4().W;
        Vector3 dest = color.ToVector3() / destA;
        Vector3 src = value.ToVector3() / srcA;
        if(color.A == 0)
        {
            dest = Vector3.Zero;
        }
        if(value.A == 0)
        {
            src = Vector3.Zero;
        }

        return new Color((src * srcA) + dest) * destA;
    }

    public static Color SubtractPreserveAlpha(this Color color, Color value)
    {
        float destA = color.ToVector4().W;
        float srcA = value.ToVector4().W;
        Vector3 dest = color.ToVector3() / destA;
        Vector3 src = value.ToVector3() / srcA;
        if(color.A == 0)
        {
            dest = Vector3.Zero;
        }
        if(value.A == 0)
        {
            src = Vector3.Zero;
        }

        return new Color(-(src * srcA) + dest) * destA;
    }
}
