using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using BloodyCloth.Ecs.Components;

namespace BloodyCloth;

public class Main : Game
{
    private static GraphicsDeviceManager _graphics;
    private static SpriteBatch _spriteBatch;
    private static ContentManager _content;
    private static Logger _logger = new();
    private static int _pixelScale = 3;
    private static Point _screenSize = new Point(640, 360);
    private static World _world;

    private SpriteFont _font;
    private RenderTarget2D _renderTarget;

    public static Logger Logger => _logger;
    public static int PixelScale => _pixelScale;
    public static Point ScreenSize => _screenSize;
    public static World World => _world;

    public static Point MousePosition => new(Mouse.GetState().X / _pixelScale, Mouse.GetState().Y / _pixelScale);
    public static Point MousePositionClamped => new(MathHelper.Clamp(Mouse.GetState().X / _pixelScale, 0, _screenSize.X - 1), MathHelper.Clamp(Mouse.GetState().Y / _pixelScale, 0, _screenSize.Y - 1));

    public static Texture2D OnePixel { get; private set; }

    public Main()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferMultiSampling = false
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _content = Content;
    }

    // for future me: it seems that in general, most things arent ready yet in the constructor, so just use Initialize <3
    protected override void Initialize()
    {
        _renderTarget = new RenderTarget2D(GraphicsDevice, _screenSize.X, _screenSize.Y);

        Window.Position = new((GraphicsDevice.DisplayMode.Width - _graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - _graphics.PreferredBackBufferHeight) / 2);

        _graphics.PreferredBackBufferWidth = _screenSize.X * _pixelScale;
        _graphics.PreferredBackBufferHeight = _screenSize.Y * _pixelScale;

        if(GraphicsDevice.DisplayMode.Height == _graphics.PreferredBackBufferHeight)
        {
            Window.Position = Point.Zero;
            Window.IsBorderless = true;
        }

        _graphics.ApplyChanges();

        _world = new World();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _world.SpriteBatch = _spriteBatch;

        OnePixel = Content.Load<Texture2D>("Images/Other/onepixel");

        _font = Content.Load<SpriteFont>("Fonts/default");

        _world.SetTile("stone", new(10, 13));
        _world.SetTile("stone", new(11, 13));
        _world.SetTile("stone", new(12, 13));
        _world.SetTile("stone", new(13, 13));
        _world.SetTile("stone", new(14, 13));

        PlayerBehavior.CreatePlayerEntity(PlayerIndex.One);
    }

    protected override void Update(GameTime gameTime)
    {
        Input.RefreshKeyboardState();
        Input.RefreshGamePadState(PlayerIndex.One);
        Input.RefreshMouseState();

        if(Input.GetPressed(Buttons.Back, PlayerIndex.One) || Input.GetPressed(Keys.Escape))
            Exit();

        _world.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

        _spriteBatch.DrawStringBold(_font, "The quick brown fox jumps over the lazy dog.", new(10, 10), Color.White, 2, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
        _spriteBatch.DrawString(_font, "The quick brown fox jumps over the lazy dog.", new(10, 20), Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

        _spriteBatch.DrawStringBold(_font, $"{_screenSize.X}x{_screenSize.Y}*{_pixelScale}", new(10, _screenSize.Y - 10), Color.White, 2, 0, Vector2.UnitY * 14, 0.5f, SpriteEffects.None, 0);
        _spriteBatch.DrawString(_font, $"{MousePosition.X},{MousePosition.Y}", new(10, _screenSize.Y - 20), Color.White, 0, Vector2.UnitY * 14, 0.5f, SpriteEffects.None, 0);

        _world.Draw(gameTime);

        _spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointWrap);
        _spriteBatch.Draw(_renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, _pixelScale, SpriteEffects.None, 0);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public static T GetContent<T>(string assetName)
    {
        return _content.Load<T>(assetName);
    }
}
