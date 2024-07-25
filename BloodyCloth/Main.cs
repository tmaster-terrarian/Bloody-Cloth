using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using BloodyCloth.GameContent;
using BloodyCloth.Graphics;
using BloodyCloth.IO;
using BloodyCloth.Utils;

using Coroutines;

using Iguina;

using LDtk;
using LDtk.Renderer;
using System.IO;
using BloodyCloth.UI;

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
    static UISystem _uiSystem;

    static LDtkFile lDtkFile;
    static LDtkWorld lDtkWorld;
    static ExampleRenderer lDtkRenderer;

    static bool paused;

    public static Point ScreenSize => Renderer.ScreenSize;
    public static Logger Logger => _logger;
    public static World World => _world;
    public static Camera Camera => _camera;
    public static CoroutineRunner Coroutines => _coroutineRunner;
    public static UISystem UISystem => _uiSystem;

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

    public static UIMenu? ActiveMenu { get; private set; }

    public static int LastPlayerHitDamage { get; set; }
    public static uint LastPlayerHitTarget { get; set; }

    public static bool InMenu { get; private set; } = false;

    public static class AppMetadata
    {
        public const string Name = "BloodyCloth";
        public const string Version = "0.1.0.0";
        public const int Build = 1;
    }

#if DEBUG
    static bool debugMode = true;
#else
    static bool debugMode = false;
#endif

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
        IsMouseVisible = false;
        IsFixedTimeStep = true;
    }

    // for future me: it seems that in general, most things arent ready yet in the constructor, so just use Initialize <3
    protected override void Initialize()
    {
        OnePixel = new Texture2D(GraphicsDevice, 1, 1);
        OnePixel.SetData<byte>([ 0xFF, 0xFF, 0xFF, 0xFF ]);

        Renderer.Initialize(_graphics, GraphicsDevice, Window);

        _camera = new Camera();

        base.Initialize();

        lDtkRenderer = new(Renderer.SpriteBatch.Base, Content);

        lDtkFile = LDtkFile.FromFile(Path.Combine(ProgramPath, "Content", "Levels", "Level0.ldtk"));

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

        // start demo project and provide our renderer and input provider.
        var uiThemeFolder = Path.Combine(ProgramPath, "Content", "UI", "Themes", "DefaultTheme");

        // create ui system
        var renderer = new UIRenderer(Content, Renderer.GraphicsDevice, uiThemeFolder);
        var input = new UIInput();
        _uiSystem = new UISystem(Path.Combine(uiThemeFolder, "system_style.json"), renderer, input);

        Player = new Player();
    }

    bool exiting = false;
    bool checkedForExit = false;

    public static void SetMenu(UIMenu instance)
    {
        ActiveMenu?.Destroy();
        ActiveMenu = instance;
    }

    public static void ForceExit()
    {
        _instance.exiting = true;

        if(_instance.checkedForExit)
            _instance.Exit();
    }

    protected override void Update(GameTime gameTime)
    {
        checkedForExit = false;

        Input.RefreshKeyboardState();
        Input.RefreshGamePadState(PlayerIndex.One);
        Input.RefreshMouseState();

        ((UIInput)_uiSystem.Input).GetKeyboardInput(gameTime);
        _uiSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        _coroutineRunner.Update(1/60f);

        if(Input.GetPressed(Keys.F1))
        {
            debugMode = !debugMode;
        }

        if((Input.GetPressed(Buttons.Back) || Input.GetPressed(Keys.Escape)) && (transitionDirection == 0 || (transitionProgress <= 0.5f && transitionDirection == -1)))
        {
            if(ActiveMenu is not null)
            {
                ActiveMenu.HandleBackButton();
            }
            else if(!InMenu)
            {
                SetMenu(new PauseMenu());
            }
        }

        if(exiting)
        {
            Exit();
            base.Update(gameTime);
            return;
        }
        else
            checkedForExit = true;

        ActiveMenu?.Update();

        paused = false;

        if(ActiveMenu is not null && ActiveMenu.PauseWhileOpen)
        {
            paused = true;
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
                if(!paused)
                {
                    FadeToNextRoom(0, FadeType.Throwback);
                }
                else
                {
                    UISystem.DebugRenderEntities = !UISystem.DebugRenderEntities;
                }
            }
        }

        if(paused)
        {
            base.Update(gameTime);
            return;
        }

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

    int fpsCounter = 0;

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

        if(Debug.Enabled)
        {
            static void DrawTexts(Vector2 offset, Color color)
            {
                Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFontBold, $"{ScreenSize.X}x{ScreenSize.Y}*{Renderer.PixelScale}", new Vector2(10, ScreenSize.Y - 10) + offset, color, 4, 0, Vector2.UnitY * 12, 1);

                Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFont, $"{MousePosition.X}, {MousePosition.Y}", new Vector2(10, ScreenSize.Y - 20) + offset, color, 4, 0, Vector2.UnitY * 12, 1);

                Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFont, $"{_world.NumCollisionChecks}", new Vector2(128, ScreenSize.Y - 10) + offset, color, 4, 0, Vector2.UnitY * 12, 1);

                Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.RegularFont, $"{LastPlayerHitDamage}", new Vector2(96, ScreenSize.Y - 10) + offset, color, 4, 0, Vector2.UnitY * 12, 1);
            }

            DrawTexts(Vector2.One, Color.Black * 0.5f);
            DrawTexts(Vector2.Zero, Color.White);
        }

        ActiveMenu?.PreDraw();

        Renderer.SpriteBatch.Base.End(); // this is hacky as shit, send help

        var renderer = (UIRenderer)_uiSystem.Renderer;
        renderer.StartFrame();
        _uiSystem.Draw();
        renderer.EndFrame();

        Renderer.SpriteBatch.Base.Begin(samplerState: SamplerState.PointWrap);

        // HUD

        ActiveMenu?.PostDraw();

        if(transitionProgress > 0)
        {
            int w = ScreenSize.X;
            int h = ScreenSize.Y;
            Random r = new(1984);
            switch(fadeType)
            {
                case FadeType.Basic:
                    Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(Point.Zero, ScreenSize), Color.Black * transitionProgress);
                    break;
                case FadeType.Throwback:
                    float p = transitionProgress;
                    Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(0, 0, w, (int)(h / 2 * p)), Color.Black); // top
                    Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(0, h - (int)(h / 2 * p), w, (int)(h / 2 * p)), Color.Black); // bottom
                    Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(0, 0, (int)(w / 2 * p), h), Color.Black); // left
                    Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(w - (int)(w / 2 * p), 0, (int)(w / 2 * p), h), Color.Black); // right
                    break;
                case FadeType.Wide:
                    Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(0, 0, w, (int)(h / 2 * transitionProgress)), Color.Black); // top
                    Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(0, h - (int)(h / 2 * transitionProgress), w, (int)(h / 2 * transitionProgress)), Color.Black); // bottom
                    break;
                case FadeType.LaggyCircle:
                    Vector2 center = (Player?.Center.ToVector2() - Camera?.Position) ?? (ScreenSize.ToVector2() / 2);
                    for(int x = -6; x < w + 6; x += 3)
                    {
                        for(int y = -6; y < h + 6; y += 3)
                        {
                            Vector2 pos = new(x, y);
                            float radiusSqr = MathUtil.Sqr(650 * (1 - transitionProgress));
                            float diff = (pos - center).LengthSquared() - radiusSqr;
                            if(diff < 0) continue;
                            float mul = 1;
                            if(diff < 1)
                                mul = diff;
                            Renderer.SpriteBatch.Base.Draw(OnePixel, pos + Vector2.One * 6, null, Color.Black * ((diff >= 0).ToInt32() * mul), (center - (pos + (Vector2.One * 6))).ToRotation(), Vector2.One * 0.5f, new Vector2(6, 12), SpriteEffects.None, 0);
                        }
                    }
                    break;
                case FadeType.Beno:
                    if(transitionDirection == 1)
                    {
                        Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(w - (int)(w * transitionProgress * 1.1f), 0, (int)(w * 1.1f), h), Color.Black);
                    }
                    else
                    {
                        Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(0, 0, (int)(w * transitionProgress), h), Color.Black);
                    }
                    break;
                case FadeType.VerticalBars:
                    if(transitionDirection == 1)
                    {
                        int w2 = MathUtil.CeilToInt(w / 6f);
                        for(int i = 0; i < 6; i++)
                        {
                            Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(w2 * i, 0, w2, (int)(h * (transitionProgress * 2 - (i / 12f) - 0.5f))), Color.Black);
                        }
                    }
                    else
                    {
                        int w2 = MathUtil.CeilToInt(w / 6f);
                        for(int i = 0; i < 6; i++)
                        {
                            Renderer.SpriteBatch.Base.Draw(OnePixel, new Rectangle(w2 * i, h - (int)(h * (transitionProgress * 2 + (i / 12f) - 0.5f)), w2, (int)(h * (transitionProgress * 2 + (i / 12f) - 0.5f))), Color.Black);
                        }
                    }
                    break;
            }
        }

        fpsCounter++;
        if(fpsCounter >= 15)
        {
            Window.Title = $"Bloody Cloth {AppMetadata.Version} [{fpsCounter * 4} FPS @ {(GC.GetTotalMemory(false) / 1048576f).ToString("F")} MB]";
            fpsCounter = 0;
        }

        Renderer.EndDrawUI();

        Renderer.FinalizeDraw();
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

    static float transitionProgress;
    static int transitionDirection = 0;
    static float currentFadeTime = 0;
    static FadeType fadeType = FadeType.Beno;
    static CoroutineHandle fadeHandler;

    public enum FadeType
    {
        Basic,
        Throwback,
        Wide,
        LaggyCircle,
        Beno,
        VerticalBars
    }

    public static void FadeToNextRoom(int room = -1, FadeType transitionStyle = FadeType.Beno, float fadeTime = 5)
    {
        if(Coroutines.IsRunning(fadeHandler))
            Coroutines.Stop(fadeHandler);

        fadeType = transitionStyle;
        currentFadeTime = fadeTime;
        fadeHandler = Coroutines.Run(TransitionFadeOut(room, fadeTime));
    }

    static IEnumerator TransitionFadeOut(int room, float fadeTime)
    {
        transitionDirection = 1;
        while(transitionProgress < 1)
        {
            transitionProgress = MathHelper.Min(1, transitionProgress + MathHelper.Max((1 - transitionProgress) / fadeTime, 0.0025f));

            yield return null;
        }

        yield return 2/60f;

        NextRoom(room);

        transitionDirection = -1;
        while(transitionProgress > 0)
        {
            transitionProgress = MathHelper.Max(0, transitionProgress - MathHelper.Max(transitionProgress / fadeTime, 0.0025f));

            yield return null;
        }

        transitionDirection = 0;
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
