using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Iguina.Defs;
using BloodyCloth.Graphics;

namespace BloodyCloth.UI;

/// <summary>
/// Provide rendering for the GUI system.
/// </summary>
/// <remarks>
/// Create the monogame renderer.
/// </remarks>
/// <param name="assetsPath">Root directory to load assets from. Check out the demo project for details.</param>
public class UIRenderer(ContentManager content, GraphicsDevice device, string assetsPath) : Iguina.Drivers.IRenderer
{
    readonly GraphicsDevice _device = device;
    readonly ContentManager _content = content;
    readonly string _assetsRoot = assetsPath;
    readonly Texture2D _whiteTexture = Main.OnePixel;

    readonly Dictionary<string, SpriteFont> _fonts = [];
    readonly Dictionary<string, Texture2D> _textures = [];

    public float GlobalTextScale = 1f;

    /// <summary>
    /// Load / get font.
    /// </summary>
    SpriteFont GetFont(string? fontName)
    {
        // var fontNameOrDefault = fontName ?? "default_font";
        // if (_fonts.TryGetValue(fontNameOrDefault, out var font)) 
        // { 
        //     return font; 
        // }

        // var ret = _content.Load<SpriteFont>(fontNameOrDefault);
        // _fonts[fontNameOrDefault] = ret;
        // return ret;

        return fontName switch
        {
            "defaultBold" => Renderer.RegularFontBold,
            "defaultItalic" => Renderer.RegularFontItalic,
            "defaultBoldItalic" => Renderer.RegularFontBoldItalic,
            "small" => Renderer.SmallFont,
            "smallBold" => Renderer.SmallFontBold,
            "default" or _ => Renderer.RegularFont
        };
    }

    /// <summary>
    /// Load / get texture.
    /// </summary>
    Texture2D GetTexture(string textureId)
    {
        if (_textures.TryGetValue(textureId, out var texture))
        {
            return texture;
        }

        var path = System.IO.Path.Combine(_assetsRoot, textureId);
        Texture2D ret = null;
        if(path.EndsWith(".xnb"))
        {
            ret = Main.LoadContent<Texture2D>(textureId.Replace(".xnb", ""));
        }
        else
        {
            ret = Texture2D.FromFile(_device, path);
        }
        _textures[textureId] = ret;
        return ret;
    }

    /// <summary>
    /// Load / get effect from id.
    /// </summary>
    Effect? GetEffect(string? effectId)
    {
        if(effectId == null) return null;
        return _content.Load<Effect>(effectId);
    }

    /// <summary>
    /// Set active effect id.
    /// </summary>
    void SetEffect(string? effectId)
    {
        if (_currEffectId != effectId)
        {
            Renderer.SpriteBatch.Base.End();
            _currEffectId = effectId;
            BeginBatch();
        }
    }
    string? _currEffectId;

    /// <summary>
    /// Convert iguina color to mg color.
    /// </summary>
    Microsoft.Xna.Framework.Color ToMgColor(Color color)
    {
        var colorMg = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
        if (color.A < 255)
        {
            float factor = (float)color.A / 255f;
            colorMg.R = (byte)((float)color.R * factor);
            colorMg.G = (byte)((float)color.G * factor);
            colorMg.B = (byte)((float)color.B * factor);
        }
        return colorMg;
    }

    /// <summary>
    /// Called at the beginning of every frame.
    /// </summary>
    public void StartFrame()
    {
        _currEffectId = null;
        _currScissorRegion = null;
        BeginBatch();
    }

    /// <summary>
    /// Called at the end of every frame.
    /// </summary>
    public void EndFrame()
    {
        Renderer.SpriteBatch.Base.End();
    }

    /// <inheritdoc/>
    public Rectangle GetScreenBounds()
    {
        Main.ScreenSize.Deconstruct(out int w, out int h);
        return new Rectangle(0, 0, w, h);
    }

    /// <inheritdoc/>
    public void DrawTexture(string? effectIdentifier, string textureId, Rectangle destRect, Rectangle sourceRect, Color color)
    {
        SetEffect(effectIdentifier);
        var texture = GetTexture(textureId);
        var colorMg = ToMgColor(color);
        Renderer.SpriteBatch.Base.Draw(texture,
            new Microsoft.Xna.Framework.Rectangle(destRect.X, destRect.Y, destRect.Width, destRect.Height),
            new Microsoft.Xna.Framework.Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height),
            colorMg);
    }

    /// <inheritdoc/>
    public Point MeasureText(string text, string? fontId, int fontSize, float spacing)
    {
        var spriteFont = GetFont(fontId);
        float scale = (fontSize / 12f) * GlobalTextScale; // 24 is the default font sprite size. you need to adjust this to your own sprite font.
        spriteFont.Spacing = spacing - 1f;
        return MeasureStringNew(spriteFont, text, scale);
    }

    /// <inheritdoc/>
    public int GetTextLineHeight(string? fontId, int fontSize)
    {
        return MeasureText("WI", fontId, fontSize, 1f).Y;
    }

    /// <inheritdoc/>

    [Obsolete("Note: currently we render outline in a primitive way. To improve performance and remove some visual artifact during transitions, its best to implement a shader that draw text with outline properly.")]
    public void DrawText(string? effectIdentifier, string text, string? fontId, int fontSize, Point position, Color fillColor, Color outlineColor, int outlineWidth, float spacing)
    {
        SetEffect(effectIdentifier);

        var spriteFont = GetFont(fontId);
        spriteFont.Spacing = spacing - 1f;
        float scale = (fontSize / 24f) * GlobalTextScale; // 24 is the default font sprite size. you need to adjust this to your own sprite font.

        // draw outline
        if ((outlineColor.A > 0) && (outlineWidth > 0))
        {
            // because we draw outline in a primitive way, we want it to fade a lot faster than fill color
            if (outlineColor.A < 255)
            {
                float alphaFactor = (float)(outlineColor.A / 255f);
                outlineColor.A = (byte)((float)fillColor.A * Math.Pow(alphaFactor, 7));
            }

            // draw outline
            var outline = ToMgColor(outlineColor);
            Renderer.SpriteBatch.Base.DrawStringSpacesFix(spriteFont, text, new Microsoft.Xna.Framework.Vector2(position.X - outlineWidth, position.Y), outline, 4, 0f, new Microsoft.Xna.Framework.Vector2(0, 0), scale, SpriteEffects.None, 0f);
            Renderer.SpriteBatch.Base.DrawStringSpacesFix(spriteFont, text, new Microsoft.Xna.Framework.Vector2(position.X, position.Y - outlineWidth), outline, 4, 0f, new Microsoft.Xna.Framework.Vector2(0, 0), scale, SpriteEffects.None, 0f);
            Renderer.SpriteBatch.Base.DrawStringSpacesFix(spriteFont, text, new Microsoft.Xna.Framework.Vector2(position.X + outlineWidth, position.Y), outline, 4, 0f, new Microsoft.Xna.Framework.Vector2(0, 0), scale, SpriteEffects.None, 0f);
            Renderer.SpriteBatch.Base.DrawStringSpacesFix(spriteFont, text, new Microsoft.Xna.Framework.Vector2(position.X, position.Y + outlineWidth), outline, 4, 0f, new Microsoft.Xna.Framework.Vector2(0, 0), scale, SpriteEffects.None, 0f);
            Renderer.SpriteBatch.Base.DrawStringSpacesFix(spriteFont, text, new Microsoft.Xna.Framework.Vector2(position.X - outlineWidth, position.Y - outlineWidth), outline, 4, 0f, new Microsoft.Xna.Framework.Vector2(0, 0), scale, SpriteEffects.None, 0f);
            Renderer.SpriteBatch.Base.DrawStringSpacesFix(spriteFont, text, new Microsoft.Xna.Framework.Vector2(position.X - outlineWidth, position.Y + outlineWidth), outline, 4, 0f, new Microsoft.Xna.Framework.Vector2(0, 0), scale, SpriteEffects.None, 0f);
            Renderer.SpriteBatch.Base.DrawStringSpacesFix(spriteFont, text, new Microsoft.Xna.Framework.Vector2(position.X + outlineWidth, position.Y - outlineWidth), outline, 4, 0f, new Microsoft.Xna.Framework.Vector2(0, 0), scale, SpriteEffects.None, 0f);
            Renderer.SpriteBatch.Base.DrawStringSpacesFix(spriteFont, text, new Microsoft.Xna.Framework.Vector2(position.X + outlineWidth, position.Y + outlineWidth), outline, 4, 0f, new Microsoft.Xna.Framework.Vector2(0, 0), scale, SpriteEffects.None, 0f);
        }

        // draw fill
        {
            var colorMg = ToMgColor(fillColor);
            Renderer.SpriteBatch.Base.DrawStringSpacesFix(spriteFont, text, new Microsoft.Xna.Framework.Vector2(position.X, position.Y), colorMg, 4, 0f, new Microsoft.Xna.Framework.Vector2(0, 0), scale, SpriteEffects.None, 0f);
        }
    }

    /// <inheritdoc/>
    public void DrawRectangle(Rectangle rectangle, Color color)
    {
        SetEffect(null);

        var texture = _whiteTexture;
        var colorMg = ToMgColor(color);
        Renderer.SpriteBatch.Base.Draw(texture,
            new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height),
            null,
            colorMg);
    }

    /// <inheritdoc/>
    public void SetScissorRegion(Rectangle region)
    {
        _currScissorRegion = region;
        _currEffectId = null;
        Renderer.SpriteBatch.Base.End();
        BeginBatch();
    }

    /// <inheritdoc/>
    public Rectangle? GetScissorRegion()
    {
        return _currScissorRegion;
    }

    // current scissor region
    Rectangle? _currScissorRegion = null;

    /// <summary>
    /// Begin a new rendering batch.
    /// </summary>
    void BeginBatch()
    {
        var effect = GetEffect(_currEffectId);
        if (_currScissorRegion != null)
        {
            _device.ScissorRectangle = new Microsoft.Xna.Framework.Rectangle(_currScissorRegion.Value.X, _currScissorRegion.Value.Y, _currScissorRegion.Value.Width, _currScissorRegion.Value.Height);
        }
        var raster = new RasterizerState
        {
            CullMode = _device.RasterizerState.CullMode,
            DepthBias = _device.RasterizerState.DepthBias,
            FillMode = _device.RasterizerState.FillMode,
            MultiSampleAntiAlias = _device.RasterizerState.MultiSampleAntiAlias,
            SlopeScaleDepthBias = _device.RasterizerState.SlopeScaleDepthBias,
            ScissorTestEnable = _currScissorRegion.HasValue
        };
        _device.RasterizerState = raster;

        Renderer.SpriteBatch.Base.Begin(samplerState: SamplerState.PointClamp, effect: effect, rasterizerState: raster);
    }

    /// <inheritdoc/>
    public void ClearScissorRegion()
    {
        _currScissorRegion = null;
        _currEffectId = null;
        Renderer.SpriteBatch.Base.End();
        BeginBatch();
    }

    /// <summary>
    /// MonoGame measure string sucks and return wrong result.
    /// So I copied the code that render string and changed it to measure instead.
    /// </summary>
    static Point MeasureStringNew(SpriteFont spriteFont, string text, float scale)
    {
        var matrix = Microsoft.Xna.Framework.Matrix.Identity;
        {
            matrix.M11 = scale;
            matrix.M22 = scale;
            matrix.M41 = 0;
            matrix.M42 = 0;
        }

        bool flag3 = true;
        var zero2 = Microsoft.Xna.Framework.Vector2.Zero;
        Point ret = new Point();
        {
            foreach (char c in text)
            {
                var _c = c;
                switch (c)
                {
                    case '\n':
                        zero2.X = 0f;
                        zero2.Y += spriteFont.LineSpacing;
                        flag3 = true;
                        continue;
                    case '\r':
                        continue;
                    case ' ':
                        _c = '0';
                        break;
                }

                var glyph = spriteFont.GetGlyphs()[_c];
                if (flag3)
                {
                    zero2.X = Math.Max(glyph.LeftSideBearing, 0f);
                    flag3 = false;
                }
                else
                {
                    zero2.X += spriteFont.Spacing + glyph.LeftSideBearing;
                }

                Microsoft.Xna.Framework.Vector2 position2 = zero2;

                position2.X += glyph.Cropping.X;
                position2.Y += glyph.Cropping.Y;
                Microsoft.Xna.Framework.Vector2.Transform(ref position2, ref matrix, out position2);
                ret.X = (int)Math.Max((float)(position2.X + (float)glyph.BoundsInTexture.Width * scale), (float)(ret.X));
                ret.Y = (int)Math.Max((float)(position2.Y + (float)spriteFont.LineSpacing * scale), (float)(ret.Y));

                zero2.X += glyph.Width + glyph.RightSideBearing;
            }
        }

        //ret.Y += spriteFont.LineSpacing / 2;
        return ret;
    }
}
