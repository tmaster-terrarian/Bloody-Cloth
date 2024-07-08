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

    private readonly struct NormalsQueueItem(Texture2D texture, Rectangle destinationRectangle, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        public Texture2D Texture { get; } = texture;
        public Rectangle DestinationRectangle { get; } = destinationRectangle;
        public Rectangle SourceRectange { get; } = sourceRectangle;
        public Color Color { get; } = color;
        public float Rotation { get; } = rotation;
        public Vector2 Origin { get; } = origin;
        public SpriteEffects Effects { get; } = effects;
        public float LayerDepth { get; } = layerDepth;
    }

    public SpriteBatch Base { get; }

    public GraphicsDevice GraphicsDevice { get; }

    public CustomSpriteBatch(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
        Base = new SpriteBatch(GraphicsDevice);

        normalMap = new RenderTarget2D(GraphicsDevice, Renderer.ScreenSize.X, Renderer.ScreenSize.Y);

        effect = Main.GetContent<Effect>("FX/NormalLit");
    }

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        Base.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        var rect = ConvertPosScale(texture.Width, texture.Height, position, scale);
        AddNormal(null, rect, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        Base.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        var rect = ConvertPosScale(texture.Width, texture.Height, position, new(scale));
        AddNormal(null, rect, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        Base.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
        AddNormal(null, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
    {
        Base.Draw(texture, position, sourceRectangle, color);
        var rect = ConvertPosScale(texture.Width, texture.Height, position, new(1));
        AddNormal(null, rect, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
    {
        Base.Draw(texture, destinationRectangle, sourceRectangle, color);
        AddNormal(null, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void Draw(Texture2D texture, Vector2 position, Color color)
    {
        Base.Draw(texture, position, color);
        var rect = ConvertPosScale(texture.Width, texture.Height, position, new(1));
        AddNormal(null, rect, null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
    {
        Base.Draw(texture, destinationRectangle, color);
        AddNormal(null, destinationRectangle, null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void DrawNormal(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        var rect = ConvertPosScale(texture.Width, texture.Height, position, scale);
        AddNormal(texture, rect, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void DrawNormal(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        var rect = ConvertPosScale(texture.Width, texture.Height, position, new(scale));
        AddNormal(texture, rect, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void DrawNormal(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        AddNormal(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void DrawNormal(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
    {
        var rect = ConvertPosScale(texture.Width, texture.Height, position, new(1));
        AddNormal(texture, rect, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void DrawNormal(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
    {
        AddNormal(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void DrawNormal(Texture2D texture, Vector2 position, Color color)
    {
        var rect = ConvertPosScale(texture.Width, texture.Height, position, new(1));
        AddNormal(texture, rect, null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void DrawNormal(Texture2D texture, Rectangle destinationRectangle, Color color)
    {
        AddNormal(texture, destinationRectangle, null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    public void Finalize(RenderTarget2D renderTarget)
    {
        Base.End();

        Base.GraphicsDevice.SetRenderTarget(normalMap);
        Base.GraphicsDevice.Clear(new Color(127, 127, 254, 255));

        Base.Begin(samplerState: SamplerState.PointWrap, transformMatrix: TransformMatrix);

        foreach(var item in normalsQueue)
        {
            if(item.Texture is null) continue;

            Base.Draw(item.Texture, item.DestinationRectangle, item.SourceRectange, item.Color, item.Rotation, item.Origin, item.Effects, item.LayerDepth);
        }

        normalsQueue.Clear();

        Base.End();

        // var view = Matrix.Multiply(TransformMatrix ?? Matrix.Identity, Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, -1, 0));

        // effect.Parameters["LightPosition"].SetValue(new Vector3(Main.MousePosition.ToVector2(), 0));
        // effect.Parameters["LightColor"].SetValue(Color.White.ToVector3());
        // effect.Parameters["LightOpacity"].SetValue(1);
        // effect.Parameters["AmbientColor"].SetValue(new Vector3(0.35f));
        // effect.Parameters["World"].SetValue(TransformMatrix ?? Matrix.Identity);
        // effect.Parameters["LightDistanceSquared"].SetValue(64);
        // effect.Parameters["ViewProjection"].SetValue(view);
        // effect.Parameters["ScreenTexture"].SetValue(renderTarget);
        // effect.Parameters["NormalTexture"].SetValue(normalMap);
        // effect.CurrentTechnique.Passes[0].Apply();

        Base.GraphicsDevice.SetRenderTarget(null);
        Base.GraphicsDevice.Clear(Color.Black);

        Base.Begin(samplerState: SamplerState.PointWrap, blendState: BlendState.Opaque);
        Base.Draw(renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Renderer.PixelScale, SpriteEffects.None, 0);
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
