using System;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using LDtk;

using BloodyCloth.Ecs.Components;
using BloodyCloth.IO;
using LDtk.Renderer;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json;
using BloodyCloth.Graphics;
using BloodyCloth.GameContent;

namespace BloodyCloth;

public class Main : Game
{
    internal const string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789*+=-–—<>_#&@%^~$.,!¡?¿:;`'\"‘’“”«»|/\\()[]{}ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜàáâãäåæçèéêëìíîïñòóôõöùúûüŸÿßẞŒœ‚„°©®™¢€£¥•…‹›";

    private static Main _instance = null;
    private static GraphicsDeviceManager _graphics;
    private static Logger _logger = new();
    private static World _world;
    private static Camera camera;

    private LDtkFile lDtkFile;
    private LDtkWorld lDtkWorld;
    private ExampleRenderer lDtkRenderer;

    public static Logger Logger => _logger;
    public static Point ScreenSize => Renderer.ScreenSize;
    public static World World => _world;
    public static bool IsPaused => !_instance.IsActive;
    public static Camera Camera => camera;

    public static Point MousePosition => new(Mouse.GetState().X / Renderer.PixelScale, Mouse.GetState().Y / Renderer.PixelScale);
    public static Point MousePositionClamped => new(MathHelper.Clamp(Mouse.GetState().X / Renderer.PixelScale, 0, ScreenSize.X - 1), MathHelper.Clamp(Mouse.GetState().Y / Renderer.PixelScale, 0, ScreenSize.Y - 1));

    public static Point WorldMousePosition => MousePosition + camera.Position.ToPoint();

    public static Texture2D OnePixel { get; private set; }
    public static Player Player { get; private set; }

    public static string SaveDataPath => new PathBuilder{AppendFinalSeparator = true}.Create(PathBuilder.LocalAppdataPath, AppMetadata.Name);
    public static string ProgramPath => AppDomain.CurrentDomain.BaseDirectory;

    public ulong ElapsedTime { get; private set; }

    public int RoomIndex { get; private set; }

    public static class AppMetadata
    {
        public const string Name = "BloodyCloth";
        public const string Version = "0.1.0.5";
        public const int Build = 5;
    }

    private static bool debugMode;
    private static bool drawTileCheckingAreas;

    public static class Debug
    {
        public static bool Enabled => debugMode;
        public static bool DrawTileCheckingAreas => drawTileCheckingAreas;
    }

    public Main()
    {
        if(_instance is not null) throw new Exception("You can't start the game more than once 4head");

        _instance = this;

        _graphics = new GraphicsDeviceManager(this)
        {
            PreferMultiSampling = false,
            SynchronizeWithVerticalRetrace = true,
            PreferredBackBufferWidth = ScreenSize.X * Renderer.PixelScale,
            PreferredBackBufferHeight = ScreenSize.Y * Renderer.PixelScale,
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
    }

    // for future me: it seems that in general, most things arent ready yet in the constructor, so just use Initialize <3
    protected override void Initialize()
    {
        OnePixel = new Texture2D(GraphicsDevice, 1, 1);
        OnePixel.SetData((byte[])[ 0xFF, 0xFF, 0xFF, 0xFF ]);

        Renderer.Initialize(_graphics, GraphicsDevice, Window);

        camera = new Camera();

        _world = new World(120, 45);

        {
            var silly = _world.Entities.Create();

            silly.GetComponent<Transform>().position = new((int)(23.5f * World.TileSize), 14 * World.TileSize);

            silly.AddComponent(new Sprite {
                texture = Content.Load<Texture2D>("Images/Tiles/stone"),
                sourceRectangle = new(Point.Zero, new(World.TileSize)),
            });

            var solid = silly.AddComponent(new Solid {
                DefaultBehavior = true,
            });
            solid.BoundingBox = new Rectangle(Point.Zero, new (World.TileSize));

            silly.AddComponent(new OscillatePosition());
        }

        base.Initialize();

        lDtkRenderer = new(Renderer.SpriteBatch);

        lDtkFile = LDtkFile.FromFile(ProgramPath + "/Content/Levels/Level0.ldtk");

        lDtkWorld = lDtkFile.LoadSingleWorld();

        foreach(var level in lDtkWorld.Levels)
        {
            lDtkRenderer.PrerenderLevel(level);
        }

        EnterRoom(lDtkWorld.Levels[0]);

        _logger.LogInfo(new PathBuilder{AppendFinalSeparator = true}.Create(PathBuilder.LocalAppdataPath, AppMetadata.Name));
        _logger.LogInfo(AppMetadata.Version);
        _logger.LogInfo(ProgramPath);
    }

    protected override void LoadContent()
    {
        Renderer.LoadContent(Content);

        Defs.Initialize();

        Player = new Player();
    }

    void EnterRoom(LDtkLevel level)
    {
        Projectile.ClearProjectiles();

        var layer = level.LayerInstances[1];
        for(int i = 0; i < layer.EntityInstances.Length; i++)
        {
            var e = layer.EntityInstances[i];
            if(e._Identifier == "JumpThrough")
                _world.JumpThroughs.Add(new(e.Px, new(e.Width, MathHelper.Max(e.Height - 1, 1))));

            if(e._Identifier.EndsWith("Slope"))
            {
                Point point1 = e.Px + new Point(layer._PxTotalOffsetX, layer._PxTotalOffsetY) + level.Position;
                Point point2 = new Point(((JsonElement)e.FieldInstances[0]._Value)[0].GetProperty("cx").GetInt32() * World.TileSize, ((JsonElement)e.FieldInstances[0]._Value)[0].GetProperty("cy").GetInt32() * World.TileSize) + new Point(layer._PxTotalOffsetX, layer._PxTotalOffsetY) + level.Position;

                if(e._Identifier == "JumpThrough_Slope")
                    _world.JumpThroughSlopes.Add(new(point1, point2, 2));
                if(e._Identifier == "Slope")
                    _world.Slopes.Add(new(point1, point2, 2));
            }
        }

        var layer2 = level.LayerInstances[4];
        for(int i = 0; i < layer2.IntGridCsv.Length; i++)
        {
            if(layer2.IntGridCsv[i] == 1) _world.SetTile(1, new(i % layer2._CWid, i / layer2._CWid));
        }
    }

    protected override void Update(GameTime gameTime)
    {
        Input.RefreshKeyboardState();
        Input.RefreshGamePadState(PlayerIndex.One);
        Input.RefreshMouseState();

        if(Input.GetPressed(Keys.F1))
        {
            debugMode = !debugMode;
        }

        if(debugMode)
        {
            if(Input.GetPressed(Keys.F2))
            {
                drawTileCheckingAreas = !drawTileCheckingAreas;
            }
        }

        if(Input.GetPressed(Buttons.Back, PlayerIndex.One) || Input.GetPressed(Keys.Escape))
            Exit();

        _world.NumCollisionChecks = 0;

        Player.Update();

        _world.Update();

        Projectile.Update();

        camera.Zoom = 1;
        camera.Position += (Player.Center.ToVector2() + new Vector2(-ScreenSize.X / 2f, -ScreenSize.Y / 2f) - camera.Position) / 4f;
        camera.Position = Vector2.Clamp(camera.Position, Vector2.Zero, (World.Bounds.Size.ToVector2() * World.TileSize) - ScreenSize.ToVector2());
        camera.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Renderer.BeginDraw(SamplerState.PointWrap, camera.Transform);

        foreach(var level in lDtkWorld.Levels)
        {
            lDtkRenderer.RenderPrerenderedLevel(level);
        }

        _world.Draw();

        Projectile.Draw();

        Player.Draw();

        // Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.RegularFontItalic, "The quick brown fox jumps over the lazy dog.", new(11, 54), Color.White, 4, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        // Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.RegularFontBold, "The quick brown fox jumps over the lazy dog.", new(10, 10), Color.White, 3, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        // Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.SmallFontBold, "The quick brown fox jumps over the lazy dog.", new(10, 22), Color.White, 2, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        // Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.RegularFont, "The quick brown fox jumps over the lazy dog.", new(10, 32), Color.White, 4, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        // Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.SmallFont, "The quick brown fox jumps over the lazy dog.", new(10, 44), Color.White, 3, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

        // Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.RegularFontBoldItalic, "The quick brown fox jumps over the lazy dog.", new(11, 66), Color.White, 4, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        // Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.RegularFontBoldItalic, "The quick brown fox jumps over the lazy dog.", new(12, 66), Color.White, 4, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

        Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.RegularFontBold, $"{ScreenSize.X}x{ScreenSize.Y}*{Renderer.PixelScale}", new Vector2(10, ScreenSize.Y - 10) + Vector2.Round(Camera.Position), Color.White, 4, 0, Vector2.UnitY * 14, 1, SpriteEffects.None, 0);
        Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.RegularFont, $"{MousePosition.X}, {MousePosition.Y}", new Vector2(10, ScreenSize.Y - 20) + Vector2.Round(Camera.Position), Color.White, 4, 0, Vector2.UnitY * 14, 1, SpriteEffects.None, 0);

        Renderer.SpriteBatch.DrawStringSpacesFix(Renderer.RegularFont, $"col checks: {_world.NumCollisionChecks}", new Vector2(128, ScreenSize.Y - 10) + Vector2.Round(Camera.Position), Color.White, 4, 0, Vector2.UnitY * 14, 1, SpriteEffects.None, 0);

        Renderer.EndDraw();

        base.Draw(gameTime);

        ElapsedTime++;
    }

    private static readonly List<string> missingAssets = [];

    public static T GetContent<T>(string assetName)
    {
        if(missingAssets.Contains(assetName)) return default;

        try
        {
            return _instance.Content.Load<T>(assetName);
        }
        catch(Exception e)
        {
            Console.Error.WriteLine(e.GetType().FullName + $": The content file \"{assetName}\" was not found.");
            missingAssets.Add(assetName);
            return default;
        }
    }

    public static void DrawLine(Point start, Point end, Color color)
    {
        VertexPositionColor[] verts = [
            new(new(start.ToVector2(), 0.2f), color),
            new(new(start.ToVector2() + Vector2.One * 2, 0.2f), color),
            new(new(end.ToVector2() + Vector2.One * 2, 0.2f), color),
            new(new(end.ToVector2(), 0.2f), color),
        ];

        Renderer.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, 1);
    }
}
