using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using LDtk;

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
    private static bool _paused;
    private static Camera camera;

    private SpriteFont _font;
    private RenderTarget2D _renderTarget;
    private Ecs.Entity player;
    private LDtkLevel level;

    public static Logger Logger => _logger;
    public static int PixelScale => _pixelScale;
    public static Point ScreenSize => _screenSize;
    public static World World => _world;
    public static bool IsPaused => _paused;
    public static Camera Camera => camera;

    public static Point MousePosition => new(Mouse.GetState().X / _pixelScale, Mouse.GetState().Y / _pixelScale);
    public static Point MousePositionClamped => new(MathHelper.Clamp(Mouse.GetState().X / _pixelScale, 0, _screenSize.X - 1), MathHelper.Clamp(Mouse.GetState().Y / _pixelScale, 0, _screenSize.Y - 1));

    public static Point WorldMousePosition => MousePosition + camera.Position.ToPoint();

    public static Texture2D OnePixel { get; private set; }

    public static string SaveDataPath => Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData,
        Environment.SpecialFolderOption.DoNotVerify
    ) + Path.DirectorySeparatorChar + AppMetadata.Name + Path.DirectorySeparatorChar;

    public static class AppMetadata
    {
        public const string Name = "BloodyCloth";
        public const string Version = "0.1.0.4";
    }

    public Main()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferMultiSampling = false,
            SynchronizeWithVerticalRetrace = true,
            PreferredBackBufferWidth = _screenSize.X * _pixelScale,
            PreferredBackBufferHeight = _screenSize.Y * _pixelScale,
        };

        _content = Content;
        _content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
    }

    // for future me: it seems that in general, most things arent ready yet in the constructor, so just use Initialize <3
    protected override void Initialize()
    {
        _renderTarget = new RenderTarget2D(GraphicsDevice, _screenSize.X, _screenSize.Y);

        Window.Position = new((GraphicsDevice.DisplayMode.Width - _graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - _graphics.PreferredBackBufferHeight) / 2);

        if(GraphicsDevice.DisplayMode.Height == _graphics.PreferredBackBufferHeight)
        {
            Window.Position = Point.Zero;
            Window.IsBorderless = true;
        }

        _graphics.ApplyChanges();

        camera = new Camera(GraphicsDevice);

        OnePixel = new Texture2D(GraphicsDevice, 1, 1);
        OnePixel.SetData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });

        _world = new World();

        _world.SetTile("stone", new(10, 13));
        _world.SetTile("stone", new(11, 13));
        _world.SetTile("stone", new(12, 13));
        _world.SetTile("stone", new(13, 13));
        _world.SetTile("stone", new(14, 13));

        {
            var silly = _world.Entities.Create();

            silly.GetComponent<Transform>().position = new((int)(15.5f * World.tileSize), 12 * World.tileSize);

            silly.AddComponent(new Sprite {
                texture = Content.Load<Texture2D>("Images/Tiles/stone"),
                sourceRectangle = new(Point.Zero, new(World.tileSize)),
            });

            var solid = silly.AddComponent(new Solid {
                DefaultBehavior = true,
            });
            solid.BoundingBox = new Rectangle(Point.Zero, new (World.tileSize));

            silly.AddComponent(new OscillatePosition());
        }

        base.Initialize();

        // file = LDtkFile.FromFile("c:/Users/Econn/AppData/Local/Programs/ldtk/extraFiles/samples/Typical_2D_platformer_example.ldtk");

        _logger.LogInfo(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)
                + Path.DirectorySeparatorChar + AppMetadata.Name + Path.DirectorySeparatorChar
        );
        _logger.LogInfo(AppMetadata.Version);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _world.SpriteBatch = _spriteBatch;

        _font = Content.Load<SpriteFont>("Fonts/default");

        player = PlayerBehavior.CreatePlayerEntity(PlayerIndex.One);
    }

    protected override void Update(GameTime gameTime)
    {
        Input.RefreshKeyboardState();
        Input.RefreshGamePadState(PlayerIndex.One);
        Input.RefreshMouseState();

        if(!IsActive && !_paused)
        {
            // pause game on unfocus
        }

        if(Input.GetPressed(Keys.F1))
        {
            _logger.LogInfo(SaveDataPath);
            _logger.LogInfo(AppMetadata.Version);
        }

        if(Input.GetPressed(Buttons.Back, PlayerIndex.One) || Input.GetPressed(Keys.Escape))
            Exit();

        _world.Update();

        camera.Zoom = 1;
        camera.Position += ((player.GetComponent<Transform>().position.ToVector2() + new Vector2(-Main.ScreenSize.X / 2f, -Main.ScreenSize.Y / 2f)) - camera.Position) / 4f;
        camera.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: camera.Transform);

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
