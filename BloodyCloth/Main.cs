using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using BloodyCloth.Ecs.Systems;

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

    public SpriteBatch SpriteBatch => _spriteBatch;

    public Main()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _content = Content;
    }

    // for future me: it seems that in general, most things arent ready yet in the constructor, so just use Initialize.
    protected override void Initialize()
    {
        _renderTarget = new RenderTarget2D(GraphicsDevice, _screenSize.X, _screenSize.Y);
        _graphics.PreferredBackBufferWidth = _screenSize.X * _pixelScale;
        _graphics.PreferredBackBufferHeight = _screenSize.Y * _pixelScale;
        _graphics.ApplyChanges();

        Window.Position = new((GraphicsDevice.DisplayMode.Width - _graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - _graphics.PreferredBackBufferHeight) / 2);

        if(GraphicsDevice.DisplayMode.Height == _graphics.PreferredBackBufferHeight)
        {
            Window.Position = Point.Zero;
            Window.IsBorderless = true;
        }

        _world = new World(GraphicsDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _font = Content.Load<SpriteFont>("Fonts/default");

        _world.Entities.Create(new Ecs.Entities.Player());
    }

    protected override void Update(GameTime gameTime)
    {
        Input.GetKeyboardState();
        Input.GetGamePadState(PlayerIndex.One);
        Input.GetJoystickState(PlayerIndex.One);
        Input.GetMouseState();

        if(Input.GetPressed(Buttons.Back, PlayerIndex.One) || Input.GetPressed(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);

        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _spriteBatch.DrawStringBold(_font, "The quick brown fox jumps over the lazy dog.", new(10, 10), Color.White, 2, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
        _spriteBatch.DrawString(_font, "The quick brown fox jumps over the lazy dog.", new(10, 20), Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

        _spriteBatch.DrawStringBold(_font, $"{_screenSize.X}x{_screenSize.Y}", new(10, _screenSize.Y - 10), Color.White, 2, 0, Vector2.UnitY * 14, 0.5f, SpriteEffects.None, 0);
        _spriteBatch.DrawString(_font, $"{MousePosition.X},{MousePosition.Y}", new(10, _screenSize.Y - 20), Color.White, 0, Vector2.UnitY * 14, 0.5f, SpriteEffects.None, 0);

        PlayerControlsSystem.Draw();
        TransformSystem.Draw();
        ActorSystem.Draw();
        SpriteSystem.Draw();

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);

        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, _pixelScale, SpriteEffects.None, 0);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public static T GetContent<T>(string assetName)
    {
        return _content.Load<T>(assetName);
    }
}
