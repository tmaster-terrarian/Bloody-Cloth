using System;
using System.Collections.Generic;
using System.Text.Json;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using BloodyCloth.GameContent;
using BloodyCloth.Graphics;
using BloodyCloth.IO;

using Coroutines;

using LDtk;
using LDtk.Renderer;

namespace BloodyCloth;

public class Main : Game
{
    public const string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789*+=-–—<>_#&@%^~$.,!¡?¿:;`'\"‘’“”«»|/\\()[]{}ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜàáâãäåæçèéêëìíîïñòóôõöùúûüŸÿßẞŒœ‚„°©®™¢€£¥•…‹›";

    static Main _instance = null;
    static GraphicsDeviceManager _graphics;
    static Logger _logger = new();
    static World _world;
    static Camera _camera;
    static readonly CoroutineRunner _coroutineRunner = new();
    static readonly CoroutineRunner _graphicsCoroutineRunner = new();

    static LDtkFile lDtkFile;
    static LDtkWorld lDtkWorld;
    static ExampleRenderer lDtkRenderer;

    public static Point ScreenSize => Renderer.ScreenSize;
    public static Logger Logger => _logger;
    public static World World => _world;
    public static Camera Camera => _camera;
    public static CoroutineRunner Coroutines => _coroutineRunner;
    public static CoroutineRunner GraphicsCoroutines => _graphicsCoroutineRunner;

    public static Point MousePosition => new(Mouse.GetState().X / Renderer.PixelScale, Mouse.GetState().Y / Renderer.PixelScale);
    public static Point MousePositionClamped => new(MathHelper.Clamp(Mouse.GetState().X / Renderer.PixelScale, 0, ScreenSize.X - 1), MathHelper.Clamp(Mouse.GetState().Y / Renderer.PixelScale, 0, ScreenSize.Y - 1));

    public static Point WorldMousePosition => MousePosition + _camera.Position.ToPoint();

    public static bool IsPaused => !_instance.IsActive;

    public static Texture2D OnePixel { get; private set; }
    public static Player Player { get; private set; }

    public static string SaveDataPath => new PathBuilder{AppendFinalSeparator = true}.Create(PathBuilder.LocalAppdataPath, AppMetadata.Name);
    public static string ProgramPath => AppDomain.CurrentDomain.BaseDirectory;

    public static ulong ElapsedTime { get; private set; }

    public static int RoomIndex { get; private set; }

    public static int LastPlayerHitDamage { get; set; }
    public static uint LastPlayerHitTarget { get; set; }

    public static class AppMetadata
    {
        public const string Name = "BloodyCloth";
        public const string Version = "0.1.0.0";
        public const int Build = 1;
    }

    static bool debugMode;
    static bool drawTileCheckingAreas;
    static bool drawLevelPadding;

    public static class Debug
    {
        public static bool Enabled => debugMode;
        public static bool DrawTileCheckingAreas => drawTileCheckingAreas;
        public static bool DrawLevelPadding => drawLevelPadding;
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
            GraphicsProfile = GraphicsProfile.HiDef,
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

        _camera = new Camera();

        base.Initialize();

        lDtkRenderer = new(Renderer.SpriteBatch.Base, Content);

        lDtkFile = LDtkFile.FromFile(ProgramPath + "/Content/Levels/Level0.ldtk");

        lDtkWorld = lDtkFile.LoadSingleWorld();

        foreach(var level in lDtkWorld.Levels)
        {
            level.WorldX = 0;
            level.WorldY = 0;

            lDtkRenderer.PrerenderLevel(level);
        }

        NextRoom(0);

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

    static void LoadLevel(LDtkLevel level)
    {
        _world?.Dispose();
        _world = new World(level.Size.Divide(World.TileSize));

        var layer = level.LayerInstances[0];
        for(int i = 0; i < layer.EntityInstances.Length; i++)
        {
            var e = layer.EntityInstances[i];
            if(e._Identifier == "PlayerStart")
                Player.Bottom = e.Px + new Point(layer._PxTotalOffsetX, layer._PxTotalOffsetY);

            if(e._Identifier == "Trigger")
            {
                Trigger.Create(
                    ((JsonElement)e.FieldInstances[0]._Value).GetString() switch
                    {
                        "NextRoom" => TriggerType.NextRoom,
                        _ => TriggerType.Invalid,
                    },
                    new(e.Px + new Point(layer._PxTotalOffsetX, layer._PxTotalOffsetY), new(e.Width, e.Height))
                );
            }

            if(e._Identifier == "Enemy")
            {
                var enemy = Enemy.CreateDirect(
                    (EnemyType)((JsonElement)e.FieldInstances[0]._Value).GetInt32(),
                    e.Px + new Point(layer._PxTotalOffsetX, layer._PxTotalOffsetY),
                    Vector2.Zero
                );

                if(enemy is not null)
                {
                    enemy.Bottom = e.Px + new Point(layer._PxTotalOffsetX, layer._PxTotalOffsetY);
                }
            }
        }

        layer = level.LayerInstances[1];
        for(int i = 0; i < layer.EntityInstances.Length; i++)
        {
            var e = layer.EntityInstances[i];
            if(e._Identifier == "JumpThrough")
                _world.JumpThroughs.Add(new(e.Px + new Point(layer._PxTotalOffsetX, layer._PxTotalOffsetY), new(e.Width, MathHelper.Max(e.Height - 1, 1))));

            if(e._Identifier.EndsWith("Slope"))
            {
                Point point1 = e.Px + new Point(layer._PxTotalOffsetX, layer._PxTotalOffsetY);
                Point point2 = new Point(((JsonElement)e.FieldInstances[0]._Value)[0].GetProperty("cx").GetInt32() * 8, ((JsonElement)e.FieldInstances[0]._Value)[0].GetProperty("cy").GetInt32() * 8) + new Point(layer._PxTotalOffsetX, layer._PxTotalOffsetY);

                if(e._Identifier == "JumpThrough_Slope")
                    _world.JumpThroughSlopes.Add(new(point1, point2, 2));
                if(e._Identifier == "Slope")
                    _world.Slopes.Add(new(point1, point2, 2));
            }
        }

        layer = level.LayerInstances[4];
        for(int i = 0; i < layer.IntGridCsv.Length; i++)
        {
            if(layer.IntGridCsv[i] == 1) _world.SetTile(1, new(i % layer._CWid, i / layer._CWid));
        }
    }

    public static void NextRoom(int room = -1)
    {
        if(room < 0) room = RoomIndex + 1;

        Projectile.ClearAll();
        Trigger.ClearAll();
        Enemy.ClearAll();

        RoomIndex = room;

        LoadLevel(lDtkWorld.Levels[RoomIndex]);

        var dummy = Enemy.CreateDirect(EnemyType.Dummy, new(24, 48), Vector2.Zero);
    }

    protected override void Update(GameTime gameTime)
    {
        Input.RefreshKeyboardState();
        Input.RefreshGamePadState(PlayerIndex.One);
        Input.RefreshMouseState();

        _coroutineRunner.Update(1/60f);

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
            if(Input.GetPressed(Keys.F3))
            {
                drawLevelPadding = !drawLevelPadding;
            }
            if(Input.GetPressed(Keys.F4))
            {
                NextRoom(0);
            }
        }

        if(Input.GetPressed(Buttons.Back, PlayerIndex.One) || Input.GetPressed(Keys.Escape))
            Exit();

        _world.NumCollisionChecks = 0;
        _world.Update();

        Pickup.Update();

        Player.Update();

        Projectile.Update();

        Enemy.Update();

        Trigger.Update();

        Camera.Zoom = 1;
        Camera.Position += (Player.Center.ToVector2() + new Vector2(-ScreenSize.X / 2f, -ScreenSize.Y / 2f) - Camera.Position) / 4f;

        Vector2 padding = new Vector2(World.TileSize, 0);
        Vector2 min = padding;
        Vector2 max = (World.Bounds.Size.ToVector2() * World.TileSize) - ScreenSize.ToVector2() - Vector2.UnitY * (_world.Height <= 23 ? (World.TileSize / 2) : 0) - padding;

        if(drawLevelPadding && debugMode)
        {
            min -= padding;
            max += padding;
        }

        Camera.Position = Vector2.Clamp(Camera.Position, min, max);

        Camera.Update();

        ElapsedTime++;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Renderer.BeginDraw(SamplerState.PointWrap, _camera.Transform);

        lDtkRenderer.RenderPrerenderedLevel(lDtkWorld.Levels[RoomIndex]);

        _world.Draw();

        Projectile.Draw();

        Enemy.Draw();

        Player.Draw();

        if(debugMode && drawLevelPadding)
        {
            Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(0, 0, World.TileSize, World.TileSize * World.Height), Color.Red * 0.25f);
            Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle((World.TileSize * World.Width) - World.TileSize, 0, World.TileSize, World.TileSize * World.Height), Color.Red * 0.25f);
        }

        Trigger.Draw();

        Renderer.EndDraw();
        Renderer.BeginDrawUI();

        _graphicsCoroutineRunner.Update(1/60f);

        if(Debug.Enabled)
        {
            static void DrawTexts(Vector2 offset, Color color)
            {
                Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFontBold, $"{ScreenSize.X}x{ScreenSize.Y}*{Renderer.PixelScale}", new Vector2(10, ScreenSize.Y - 10) + offset, color, 4, 0, Vector2.UnitY * 12, 1);
                Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFont, $"{MousePosition.X}, {MousePosition.Y}", new Vector2(10, ScreenSize.Y - 20) + offset, color, 4, 0, Vector2.UnitY * 12, 1);

                Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFont, $"{_world.NumCollisionChecks}", new Vector2(128, ScreenSize.Y - 10) + offset, color.Subtract(Color.White), 4, 0, Vector2.UnitY * 12, 1);

                Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFont, $"{LastPlayerHitDamage}", new Vector2(96, ScreenSize.Y - 10) + offset, color, 4, 0, Vector2.UnitY * 12, 1);
            }

            DrawTexts(Vector2.One, Color.Black * 0.5f);
            DrawTexts(Vector2.Zero, Color.White);
        }

        Color col1 = new Color(100, 150, 200) * 1f;
        Color col2 = new Color(100, 100, 100) * (100f / 255f);
        Color blend = col1.AddPreserveAlpha(Color.Transparent);

        Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFont, $"{col1}", new Vector2(144, ScreenSize.Y - 30), col1, 4, 0, Vector2.UnitY * 12);
        Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFont, $"{col2}", new Vector2(144, ScreenSize.Y - 20), col2, 4, 0, Vector2.UnitY * 12);
        Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFont, $"{blend}", new Vector2(144, ScreenSize.Y - 10), blend, 4, 0, Vector2.UnitY * 12);

        Renderer.EndDrawUI();
        Renderer.FinalizeDraw();
    }

    private static readonly List<string> missingAssets = [];

    public static T LoadContent<T>(string assetName)
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
}
