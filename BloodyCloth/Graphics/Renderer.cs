using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.Graphics;

public static class Renderer
{
    private static GraphicsDeviceManager _graphics;
    private static SpriteBatch _spriteBatch;
    private static RenderTarget2D _renderTarget;

    private static int _pixelScale = 3;
    private static Point _screenSize = new Point(640, 360);

    public static GraphicsDevice GraphicsDevice { get; private set; }
    public static SpriteBatch SpriteBatch => _spriteBatch;
    public static RenderTarget2D RenderTarget => _renderTarget;

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

        EmptyTexture = new Texture2D(GraphicsDevice, 1, 1);
        EmptyTexture.SetData((byte[])[0, 0, 0, 0]);

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
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        RegularFont = content.Load<SpriteFont>("Fonts/default");
        RegularFontBold = content.Load<SpriteFont>("Fonts/defaultBold");
        RegularFontItalic = content.Load<SpriteFont>("Fonts/defaultItalic");
        RegularFontBoldItalic = content.Load<SpriteFont>("Fonts/defaultItalic");

        RegularFontItalic.Spacing = -1;
        RegularFontBoldItalic.Spacing = 1;

        SmallFont = content.Load<SpriteFont>("Fonts/small");
        SmallFontBold = content.Load<SpriteFont>("Fonts/smallBold");
    }

    public static void BeginDraw(SamplerState samplerState = null, Matrix? transformMatrix = null)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(samplerState: samplerState, transformMatrix: transformMatrix);
    }

    public static void EndDraw()
    {
        SpriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
        SpriteBatch.Draw(_renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, _pixelScale, SpriteEffects.None, 0);
        SpriteBatch.End();
    }
}
