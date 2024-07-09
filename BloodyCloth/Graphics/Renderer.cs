using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.Graphics;

public static class Renderer
{
    static GraphicsDeviceManager _graphics;
    static CustomSpriteBatch _spriteBatch;
    static RenderTarget2D _renderTarget;
    static RenderTarget2D _uiRenderTarget;

    static int _pixelScale = 3;
    static Point _screenSize = new Point(640, 360);

    // static Effect effect;

    public static GraphicsDevice GraphicsDevice { get; private set; }
    public static CustomSpriteBatch SpriteBatch => _spriteBatch;
    public static RenderTarget2D RenderTarget => _renderTarget;
    public static RenderTarget2D UIRenderTarget => _uiRenderTarget;

    public static GameWindow Window { get; private set; }

    public static int PixelScale => _pixelScale;
    public static Point ScreenSize => _screenSize;

    public static SpriteFont RegularFont { get; private set; }
    public static SpriteFont RegularFontBold { get; private set; }
    public static SpriteFont RegularFontItalic { get; private set; }
    public static SpriteFont RegularFontBoldItalic { get; private set; }

    public static SpriteFont SmallFont { get; private set; }
    public static SpriteFont SmallFontBold { get; private set; }

    /// <summary>
    /// Represents a missing (empty) <see cref="Texture2D"/>.
    /// </summary>
    public static Texture2D EmptyTexture { get; private set; }

    public static void Initialize(GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, GameWindow window)
    {
        _graphics = graphics;
        GraphicsDevice = graphicsDevice;
        Window = window;

        _renderTarget = new RenderTarget2D(GraphicsDevice, _screenSize.X, _screenSize.Y);
        _uiRenderTarget = new RenderTarget2D(GraphicsDevice, _screenSize.X, _screenSize.Y);

        EmptyTexture = new Texture2D(GraphicsDevice, 1, 1);
        EmptyTexture.SetData<byte>([0, 0, 0, 0]);

        Window.Position = new((GraphicsDevice.DisplayMode.Width - _graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - _graphics.PreferredBackBufferHeight) / 2);

        if(GraphicsDevice.DisplayMode.Height == _graphics.PreferredBackBufferHeight)
        {
            Window.Position = Point.Zero;
            Window.IsBorderless = true;
        }

        _graphics.ApplyChanges();
    }

    public static void LoadContent(ContentManager content)
    {
        _spriteBatch = new CustomSpriteBatch(GraphicsDevice);

        RegularFont = content.Load<SpriteFont>("Fonts/default");
        RegularFontBold = content.Load<SpriteFont>("Fonts/defaultBold");
        RegularFontItalic = content.Load<SpriteFont>("Fonts/defaultItalic");
        RegularFontBoldItalic = content.Load<SpriteFont>("Fonts/defaultItalic");

        RegularFontItalic.Spacing = -1;
        RegularFontBoldItalic.Spacing = 1;

        SmallFont = content.Load<SpriteFont>("Fonts/small");
        SmallFontBold = content.Load<SpriteFont>("Fonts/smallBold");

        // effect = content.Load<Effect>("FX/NormalLit");
    }

    public static void BeginDraw(SamplerState samplerState = null, Matrix? transformMatrix = null, SpriteSortMode sortMode = SpriteSortMode.Deferred)
    {
        GraphicsDevice.SetRenderTarget(RenderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Base.Begin(sortMode: sortMode, samplerState: samplerState, transformMatrix: transformMatrix);
        SpriteBatch.TransformMatrix = transformMatrix;
    }

    public static void EndDraw()
    {
        SpriteBatch.Base.End();
    }

    public static void BeginDrawUI(Point? canvasSize = null)
    {
        Point size = canvasSize ?? Point.Zero;
        if(canvasSize is not null && size != Point.Zero && _uiRenderTarget.Bounds.Size != new Point(Math.Abs(size.X), Math.Abs(size.Y)))
        {
            _uiRenderTarget = new RenderTarget2D(GraphicsDevice, Math.Abs(size.X), Math.Abs(size.Y));
        }

        GraphicsDevice.SetRenderTarget(UIRenderTarget);
        GraphicsDevice.Clear(Color.Black * 0);

        SpriteBatch.Base.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointWrap);
    }

    public static void EndDrawUI()
    {
        SpriteBatch.Base.End();
    }

    public static void FinalizeDraw()
    {
        SpriteBatch.Finalize(RenderTarget);

        SpriteBatch.Base.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
        SpriteBatch.Base.Draw(UIRenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, PixelScale, SpriteEffects.None, 0);
        SpriteBatch.Base.End();
    }
}
