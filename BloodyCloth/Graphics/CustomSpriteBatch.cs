using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.Graphics;

public class CustomSpriteBatch
{
    private readonly List<NormalsQueueItem> normalsQueue = [];
    private RenderTarget2D normalMap;

    private Effect effect;

    public RenderTarget2D NormalMap => normalMap;

    public Matrix? TransformMatrix { get; set; }

    public SpriteBatch Base { get; }

    public GraphicsDevice GraphicsDevice { get; }

    private readonly struct NormalsQueueItem(Texture2D texture, Rectangle destinationRectangle, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        public Texture2D Texture { get; } = texture;
        public Rectangle DestinationRectangle { get; } = destinationRectangle;
        public Rectangle SourceRectange { get; } = sourceRectangle;
        public Color Color { get; } = color;
        public float Rotation { get; } = rotation;
        public Vector2 Origin { get; } = origin;
        public SpriteEffects SpriteEffects { get; } = effects;
        public float LayerDepth { get; } = layerDepth;
    }

    public CustomSpriteBatch(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
        Base = new SpriteBatch(GraphicsDevice);

        normalMap = new RenderTarget2D(GraphicsDevice, Renderer.ScreenSize.X, Renderer.ScreenSize.Y);

        effect = Main.LoadContent<Effect>("FX/NormalLit2");
    }

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Base.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
    }

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Base.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Base.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Base.Draw(texture, position, sourceRectangle, color);
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Base.Draw(texture, destinationRectangle, sourceRectangle, color);
    }

    public void Draw(Texture2D texture, Vector2 position, Color color)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Base.Draw(texture, position, color);
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Base.Draw(texture, destinationRectangle, color);
    }

    public void DrawNormal(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var rect = ConvertPosScale(texture.Width, texture.Height, position, scale);
        AddNormal(texture, rect, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void DrawNormal(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var rect = ConvertPosScale(texture.Width, texture.Height, position, new(scale));
        AddNormal(texture, rect, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void DrawNormal(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(texture);

        AddNormal(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void DrawNormal(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var rect = ConvertPosScale(texture.Width, texture.Height, position, new(1));
        AddNormal(texture, rect, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void DrawNormal(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
    {
        ArgumentNullException.ThrowIfNull(texture);

        AddNormal(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void DrawNormal(Texture2D texture, Vector2 position, Color color)
    {
        ArgumentNullException.ThrowIfNull(texture);

        var rect = ConvertPosScale(texture.Width, texture.Height, position, new(1));
        AddNormal(texture, rect, null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void DrawNormal(Texture2D texture, Rectangle destinationRectangle, Color color)
    {
        ArgumentNullException.ThrowIfNull(texture);

        AddNormal(texture, destinationRectangle, null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float width = 1, float layerDepth = 0)
    {
        Vector2 pivot = new(0, 0.5f);
        Texture2D tex = Main.OnePixel;

        float length = Vector2.Distance(start, end);

        Base.Draw(tex, start, null, color, (end - start).ToRotation(), pivot, new Vector2(length, width), SpriteEffects.None, layerDepth);
    }

    public void DrawLine(Vector2 start, float direction, float length, Color color, float width = 1, float layerDepth = 0)
    {
        Vector2 pivot = new(0, 0.5f);
        Texture2D tex = Main.OnePixel;

        Base.Draw(tex, start, null, color, direction, pivot, new Vector2(length, width), SpriteEffects.None, layerDepth);
    }

    public void FinalizeDraw()
    {
        Renderer.GraphicsDevice.SetRenderTarget(normalMap);
        Renderer.GraphicsDevice.Clear(new Color(128, 128, 255, 255));

        Base.Begin(samplerState: SamplerState.PointWrap, transformMatrix: TransformMatrix);

        DrawNormal(Main.LoadContent<Texture2D>("Images/Levels/Tower/Bricks_Normal"), Vector2.Zero, Color.White);

        foreach(var item in normalsQueue)
        {
            if(item.Texture is null) continue;

            Base.Draw(item.Texture, item.DestinationRectangle, item.SourceRectange, item.Color, item.Rotation, item.Origin, item.SpriteEffects, item.LayerDepth);
        }

        normalsQueue.Clear();

        Base.End();

        Matrix world = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

        var view = Matrix.Multiply(Matrix.Multiply(world, TransformMatrix ?? Matrix.Identity), Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, -1, 0));

        // first attempt
        // effect.Parameters["LightPosition"].SetValue(new Vector3(Main.MousePosition.ToVector2(), 0));
        // effect.Parameters["LightColor"].SetValue(Color.White.ToVector3());
        // effect.Parameters["LightOpacity"].SetValue(1);
        // effect.Parameters["AmbientColor"].SetValue(new Vector3(0.35f));
        // effect.Parameters["World"].SetValue(world * (TransformMatrix ?? Matrix.Identity));
        // effect.Parameters["LightDistanceSquared"].SetValue(64);
        // effect.Parameters["ViewProjection"].SetValue(view);
        // effect.Parameters["ScreenTexture"].SetValue(renderTarget);
        // effect.Parameters["NormalTexture"].SetValue(normalMap);
        // effect.CurrentTechnique.Passes[0].Apply();

        // second attempt
        // effect.Parameters["World"].SetValue(world);
        // effect.Parameters["ViewProjection"].SetValue(view);
        // effect.Parameters["SpriteTexture"].SetValue(renderTarget);
        // effect.Parameters["NormalMap"].SetValue(normalMap);
        // effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
        // effect.CurrentTechnique.Passes[0].Apply();

        Renderer.GraphicsDevice.SetRenderTarget(null);
        Renderer.GraphicsDevice.Clear(Color.Black);

        Base.Begin(samplerState: SamplerState.PointWrap, blendState: BlendState.Opaque);
        Base.Draw(Renderer.RenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Renderer.PixelScale, SpriteEffects.None, 0);
        Base.End();
    }

    static Rectangle ConvertPosScale(int width, int height, Vector2 position, Vector2 scale)
    {
        return new Rectangle(
            (int)position.X, (int)position.Y,
            (int)(width * scale.X), (int)(height * scale.Y)
        );
    }

    void AddNormal(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        Rectangle srcRect = sourceRectangle ?? new Rectangle(0, 0, texture?.Width ?? 1, texture?.Height ?? 1);
        normalsQueue.Add(new(texture, destinationRectangle, srcRect, color, rotation, origin, effects, layerDepth));
    }
}
