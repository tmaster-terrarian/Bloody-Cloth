using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Ecs;
using BloodyCloth.Ecs.Components;

namespace BloodyCloth;

public class World : IDisposable
{
    private Rectangle[,] _collisions = null;
    private EntityWorld _entityWorld = new();

    public SpriteBatch SpriteBatch { get; set; }

    private readonly int[,] _tiles;

    public const int TileSize = 8;

    private readonly int width;
    private readonly int height;

    public bool Visible { get; set; } = true;

    public EntityWorld Entities => _entityWorld;

    public Rectangle Bounds {
        get {
            return new Rectangle(0, 0, Width, Height);
        }
    }
    public Point Size {
        get {
            return new Point(Width, Height);
        }
    }

    public Rectangle[,] Collisions {
        get {
            if(_collisions != null) return _collisions;

            Rectangle[,] rectangles = new Rectangle[Width, Height];

            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    var tile = _tiles[x, y];
                    Rectangle rect = new Rectangle(-10000, -10000, 1, 1);

                    if(tile != 0)
                        rect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);

                    rectangles[x, y] = rect;
                }
            }

            _collisions = rectangles;
            return rectangles;
        }
    }

    public List<Rectangle> JumpThroughs { get; } = new();
    public List<Triangle> JumpThroughSlopes { get; } = new();
    public List<Triangle> Slopes { get; } = new();

    public int Width => width;
    public int Height => height;

    public World(int width, int height)
    {
        this.width = MathHelper.Max(width, 80);
        this.height = MathHelper.Max(height, 45);
        _tiles = new int[this.width, this.width];

        _collisions = new Rectangle[this.width, this.width];
    }

    public Rectangle ValidateArea(Rectangle rectangle)
    {
        return new(
            Math.Clamp(rectangle.X, 0, Width - 1),
            Math.Clamp(rectangle.Y, 0, Height - 1),
            Math.Clamp(rectangle.X + rectangle.Width, rectangle.X + 1, Width) - rectangle.X,
            Math.Clamp(rectangle.Y + rectangle.Height, rectangle.Y + 1, Height) - rectangle.Y
        );
    }

    public void RefreshTileShapes(Rectangle area)
    {
        var _area = ValidateArea(area);

        for(int x = _area.X; x < _area.X + _area.Width; x++)
        {
            for(int y = _area.Y; y < _area.Y + _area.Height; y++)
            {
                int tile = _tiles[x, y];

                Rectangle rect = Rectangle.Empty;
                if(tile != 0)
                    rect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
                _collisions[x, y] = rect;
            }
        }
    }

    public void Update()
    {
        ComponentSystems.Update();
    }

    public void Draw()
    {
        if(SpriteBatch is null) return;

        for(int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                int tile = _tiles[x, y];
                if(tile == 0) continue;

                if(Main.DebugMode) SpriteBatch.Draw(Main.GetContent<Texture2D>("Images/Other/tileOutline"), _collisions[x, y], Color.Red * 0.5f);
            }
        }

        if(!Visible) return;

        if(Main.DebugMode)
        {
            foreach(var rect in JumpThroughs)
            {
                SpriteBatch.Draw(Main.OnePixel, rect, Color.LimeGreen * 0.5f);
            }

            foreach(var tri in JumpThroughSlopes)
            {
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(tri.P1 - new Point(1), new(2)), Color.LimeGreen * 0.75f);
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(tri.P2 - new Point(1), new(2)), Color.LimeGreen * 0.75f);
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(tri.P3 - new Point(1), new(2)), Color.LimeGreen * 0.75f);

                Point min = new(MathHelper.Min(MathHelper.Min(tri.P1.X, tri.P2.X), tri.P3.X), MathHelper.Min(MathHelper.Min(tri.P1.Y, tri.P2.Y), tri.P3.Y));
                Point max = new(MathHelper.Max(MathHelper.Max(tri.P1.X, tri.P2.X), tri.P3.X), MathHelper.Max(MathHelper.Max(tri.P1.Y, tri.P2.Y), tri.P3.Y));

                for(int x = 0; x < max.X - min.X; x++)
                {
                    for(int y = 0; y < max.Y - min.Y; y++)
                    {
                        var p = new Point(x, y) + min;

                        if(tri.Contains(p))
                        {
                            SpriteBatch.Draw(Main.OnePixel, new Rectangle(p, new(1)), Color.LimeGreen * 0.5f);
                        }
                    }
                }
            }

            foreach(var tri in Slopes)
            {
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(tri.P1 - new Point(1), new(2)), Color.Red * 0.75f);
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(tri.P2 - new Point(1), new(2)), Color.Red * 0.75f);
                SpriteBatch.Draw(Main.OnePixel, new Rectangle(tri.P3 - new Point(1), new(2)), Color.Red * 0.75f);

                Point min = new(MathHelper.Min(MathHelper.Min(tri.P1.X, tri.P2.X), tri.P3.X), MathHelper.Min(MathHelper.Min(tri.P1.Y, tri.P2.Y), tri.P3.Y));
                Point max = new(MathHelper.Max(MathHelper.Max(tri.P1.X, tri.P2.X), tri.P3.X), MathHelper.Max(MathHelper.Max(tri.P1.Y, tri.P2.Y), tri.P3.Y));

                for(int x = 0; x < max.X - min.X; x++)
                {
                    for(int y = 0; y < max.Y - min.Y; y++)
                    {
                        var p = new Point(x, y) + min;

                        if(tri.Contains(p))
                        {
                            SpriteBatch.Draw(Main.OnePixel, new Rectangle(p, new(1)), Color.Red * 0.5f);
                        }
                    }
                }
            }

            foreach(var entity in GetAllEntitiesWithComponent<Solid>())
            {
                SpriteBatch.Draw(Main.OnePixel, entity.GetComponent<Solid>().WorldBoundingBox, Color.Orange * 0.5f);
            }

            foreach(var actor in GetAllEntitiesWithComponent<Actor>())
            {
                SpriteBatch.Draw(Main.OnePixel, actor.GetComponent<Actor>().WorldBoundingBox, Color.Red * 0.5f);
            }
        }

        ComponentSystems.Draw();
    }

    public void DrawSprite(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
    {
        SpriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
    }

    public void DrawSprite(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
    {
        SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
    }

    public void DrawSprite(Sprite sprite, Transform transform)
    {
        SpriteBatch.Draw(sprite.texture, transform.position.ToVector2(), sprite.sourceRectangle, sprite.color, transform.rotation, sprite.origin.ToVector2(), transform.scale, sprite.spriteEffects, sprite.LayerDepth);
    }

    public void Dispose()
    {
        _entityWorld.Dispose();
        _entityWorld = null;

        GC.SuppressFinalize(this);
    }

    public Ecs.Entity? GetEntityWithId(uint id) => _entityWorld.GetEntityWithId(id);

    public List<Actor> GetAllActorComponents()
    {
        List<Actor> actors = new();
        foreach(var actor in ActorSystem.Components)
        {
            if(!actor.IsEnabled) continue;

            actors.Add(actor);
        }
        return actors;
    }

    public List<Ecs.Entity> GetAllEntitiesWithComponent<T>() where T : Component
    {
        List<Ecs.Entity> entities = new();
        foreach(var entity in _entityWorld.Entities)
        {
            if(!entity.IsEnabled) continue;

            if(entity.HasComponent<T>()) entities.Add(entity);
        }
        return entities;
    }

    public bool TileMeeting(Rectangle rect)
    {
        Rectangle[,] cols = Collisions;
        for(int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                var r = cols[x, y];
                if(r == Rectangle.Empty) continue;

                if(rect.Intersects(r)) return true;
            }
        }
        foreach(var tri in Slopes)
        {
            if(tri.Intersects(rect)) return true;
        }
        return false;
    }

    public Rectangle? JumpThroughPlace(Rectangle bbox)
    {
        foreach(var rect in JumpThroughs)
        {
            if(rect.Intersects(bbox)) return rect;
        }
        return null;
    }

    public Triangle? JumpThroughSlopePlace(Rectangle bbox)
    {
        foreach(var tri in JumpThroughSlopes)
        {
            if(tri.Intersects(bbox)) return tri;
        }
        return null;
    }

    public bool JumpThroughMeeting(Rectangle rect) => JumpThroughPlace(rect) is not null || JumpThroughSlopePlace(rect) is not null;

    public Solid? SolidPlace(Rectangle bbox)
    {
        foreach(var entity in _entityWorld.Entities)
        {
            if(!entity.IsEnabled) continue;

            Solid solid = entity.GetComponent<Solid>();
            if(solid is not null)
            {
                if(solid.Collidable && solid.WorldBoundingBox.Intersects(new(bbox.Location, bbox.Size))) return solid;
            }
        }
        return null;
    }

    public bool SolidMeeting(Rectangle bbox) => SolidPlace(bbox) is not null;

    public void SetTile(int id, Point position)
    {
        if(!InWorld(position.X, position.Y)) return;

        _tiles[position.X, position.Y] = id;

        RefreshTileShapes(new Rectangle(position.X - 1, position.Y - 1, 3, 3));
    }

    public static bool InWorld(World level, int x, int y)
    {
        return x >= 0 && x < level.Width && y >= 0 && y < level.Height;
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
