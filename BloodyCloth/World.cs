using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Ecs;
using BloodyCloth.Ecs.Systems;
using System.Linq;
using BloodyCloth.Ecs.Components;

namespace BloodyCloth;

public class World : IDisposable, IDrawable
{
    private Rectangle[,] _collisions = null;

    private EntityWorld _entityWorld = new();

    private Dictionary<string, Texture2D> _textureCache;

    private SpriteBatch _spriteBatch;

    protected Tile[,] _tiles;

    public const int tileSize = 8;

    public int width;
    public int height;

    public int DrawOrder => 0;

    public bool Visible => true;

    public EntityWorld Entities => _entityWorld;

    public Rectangle Bounds {
        get {
            return new Rectangle(0, 0, width, height);
        }
    }
    public Point Size {
        get {
            return new Point(width, height);
        }
    }

    public Rectangle[,] Collisions {
        get {
            if(_collisions != null) return _collisions;

            Rectangle[,] rectangles = new Rectangle[width, height];

            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    var tile = _tiles[x, y];
                    Rectangle rect = new Rectangle(-10000, -10000, 1, 1);

                    if(tile.id != "air")
                        rect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);

                    rectangles[x, y] = rect;
                }
            }

            _collisions = rectangles;
            return rectangles;
        }
    }

    public Tile TilePlace(Rectangle rect)
    {
        Rectangle[,] cols = Collisions;
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                var r = cols[x, y];

                if(rect.Intersects(r)) return _tiles[x, y];
            }
        }
        return null;
    }

    public bool TileMeeting(Rectangle rect) => TilePlace(rect) != null;

    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;

    public World(GraphicsDevice graphicsDevice)
    {
        _spriteBatch = new(graphicsDevice);

        width = 40;
        height = 23;
        _tiles = new Tile[width, height];

        _textureCache = new Dictionary<string, Texture2D>
        {
            { "air", null }
        };

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                _tiles[x, y] = new Tile();
            }
        }

        _collisions = new Rectangle[width, height];
    }

    public Rectangle ValidateArea(Rectangle rectangle)
    {
        var area = new Rectangle(rectangle.Location, rectangle.Size);

        return new(
            Math.Clamp(rectangle.X, 0, width - 1),
            Math.Clamp(rectangle.Y, 0, height - 1),
            Math.Clamp(rectangle.X + rectangle.Width, rectangle.X + 1, width) - rectangle.X,
            Math.Clamp(rectangle.Y + rectangle.Height, rectangle.Y + 1, height) - rectangle.Y
        );
    }

    public void RefreshTileShapes(Rectangle area)
    {
        var _area = ValidateArea(area);

        for(int x = _area.X; x < _area.X + _area.Width; x++)
        {
            for(int y = _area.Y; y < _area.Y + _area.Height; y++)
            {
                Tile tile = _tiles[x, y];

                Rectangle rect = new Rectangle(-10000, -10000, 1, 1);
                if(tile.id != "air")
                    rect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                _collisions[x, y] = rect;

                if(tile.id == "air") continue;

                RefreshTileShape(tile, x, y);
            }
        }
    }

    private void RefreshTileShape(Tile tile, int x, int y)
    {
        byte matches = 0b0000;

        if((y > 0 && _tiles[x, y - 1].id == tile.id) || y <= 0)
            matches |= 0b0001;

        if((x < width - 1 && _tiles[x + 1, y].id == tile.id) || x >= width - 1)
            matches |= 0b0010;

        if((y < height - 1 && _tiles[x, y + 1].id == tile.id) || y >= height - 1)
            matches |= 0b0100;

        if((x > 0 && _tiles[x - 1, y].id == tile.id) || x <= 0)
            matches |= 0b1000;

        tile.shape = matches;
    }

    public void LoadContent()
    {
        
    }

    public void Update()
    {
        PlayerControlsSystem.Update();
        TransformSystem.Update();
        ActorSystem.Update();
        SpriteSystem.Update();
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Tile tile = _tiles[x, y];
                if(tile.id == "air") continue;

                Texture2D texture = Main.GetContent<Texture2D>("Images/Tiles/" + tile.id);
                if(texture == null) continue;

                Rectangle UV = Tile.GetShapeUV(tile.shape);
                UV.X *= tileSize;
                UV.Y *= tileSize;
                UV.Width *= tileSize;
                UV.Height *= tileSize;

                _spriteBatch.Draw(
                    texture,
                    new Vector2(x * tileSize, y * tileSize),
                    UV,
                    Color.White,
                    0,
                    Vector2.Zero,
                    new Vector2(1, 1),
                    SpriteEffects.None,
                    0f
                );
            }
        }

        PlayerControlsSystem.Draw();
        TransformSystem.Draw();
        ActorSystem.Draw();
        SpriteSystem.Draw();

        _spriteBatch.End();
    }

    public void DrawSprite(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        _spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
    }

    public void DrawSprite(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
    {
        _spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void DrawSprite(Ecs.Components.Sprite sprite, Ecs.Components.Transform transform)
    {
        _spriteBatch.Draw(sprite.texture, transform.position.ToVector2(), sprite.sourceRectangle, sprite.color, transform.rotation, sprite.origin.ToVector2(), transform.scale, sprite.spriteEffects, sprite.LayerDepth);
    }

    public void Dispose()
    {
        _textureCache.Clear();
        _textureCache = null;

        _entityWorld.Dispose();
        _entityWorld = null;

        GC.SuppressFinalize(this);
    }

    public Entity? GetEntityWithId(uint id) => _entityWorld.GetEntityWithId(id);

    public List<Entity> GetAllEntitiesWithComponent<T>() where T : Component
    {
        List<Entity> entities = new();
        foreach(var entity in _entityWorld.Entities)
        {
            if(entity.HasComponent<T>()) entities.Add(entity);
        }
        return entities;
    }

    public Entity? SolidPlace(Rectangle bbox, Point position)
    {
        foreach(var entity in _entityWorld.Entities)
        {
            Solid solid = entity.GetComponent<Solid>();
            if(solid is not null)
            {
                if(solid.WorldBoundingBox.Intersects(new(bbox.Location + position, bbox.Size))) return entity;
            }
        }
        return null;
    }

    public bool SolidMeeting(Rectangle bbox, Point position) => SolidPlace(bbox, position) is not null;

    public string GetTileIdAtPosition(Vector2 position) => GetTileIdAtTilePosition((position / tileSize).ToPoint());

    public string GetTileIdAtTilePosition(Point position)
    {
        int x = position.X;
        int y = position.Y;

        if(!InWorld(x, y)) return "air";

        return _tiles[x, y].id;
    }

    public Tile? GetTileAtPosition(Vector2 position) => GetTileAtTilePosition((position / tileSize).ToPoint());

    public Tile? GetTileAtTilePosition(Point position)
    {
        int x = position.X;
        int y = position.Y;

        if(!InWorld(x, y)) return null;

        return _tiles[x, y];
    }

    public void SetTile(string id, Point position)
    {
        if(!InWorld(position.X, position.Y)) return;

        _tiles[position.X, position.Y] = new Tile(id);

        RefreshTileShapes(new Rectangle(new Point(position.X - 1, position.Y - 1), new Point(3, 3)));
    }

    public static bool InWorld(World level, int x, int y)
    {
        return x >= 0 && x < level.width && y >= 0 && y < level.height;
    }

    public static bool InWorld(World level, Point pos)
    {
        return InWorld(level, pos.X, pos.Y);
    }

    public bool InWorld(int x, int y)
    {
        return InWorld(this, x, y);
    }

    public bool InWorld(Point pos)
    {
        return InWorld(pos.X, pos.Y);
    }
}
