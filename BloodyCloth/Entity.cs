using System;
using Microsoft.Xna.Framework;

namespace BloodyCloth;

public abstract class Entity
{
    // alright lets not make it stupid complicated this time

    public const int maxEnemies = 1000;

    public static readonly Entity[] enemy = new Entity[maxEnemies];

    int _layerDepth = 0;

    public Color color = Color.White;

    public float LayerDepth
    {
        get => 1 - ((float)(_layerDepth + 10000) / 20000);
        set => _layerDepth = Extensions.Floor((1 - MathHelper.Clamp(value, 0, 1)) * 20000 - 10000);
    }

    public int width = 8;
    public int height = 8;

    public virtual bool Active { get; private set; }

    public Point Size => new(width, height);

    public float rotation;
    public Point position;
    public Vector2 velocity;

    protected Vector2 drawScale = Vector2.One;

    public int facing = 1;

    public Point Center {
        get => new(position.X + (width/2), position.Y + (height/2));
        set {
            position = new(value.X - (width/2), value.Y - (height/2));
        }
    }

    public Point TopLeft {
        get => position;
        set {
            position = value;
        }
    }

    public Point TopRight {
        get => new(position.X + width, position.Y);
        set {
            position = new(value.X - width, value.Y);
        }
    }

    public Point BottomLeft {
        get => new(position.X, position.Y + height);
        set {
            position = new(value.X, value.Y - height);
        }
    }

    public Point BottomRight {
        get => new(position.X + width, position.Y + height);
        set {
            position = new(value.X - width, value.Y - height);
        }
    }

    public Rectangle Hitbox {
        get => new(position, Size);
        set {
            position = value.Location;
            width = value.Width;
            height = value.Height;
        }
    }

    public Point Left {
        get => new(position.X, position.Y + (height/2));
        set {
            position = new(value.X, value.Y - (height/2));
        }
    }

    public Point Right {
        get => new(position.X + width, position.Y + (height/2));
        set {
            position = new(value.X - width, value.Y - (height/2));
        }
    }

    public Point Top {
        get => new(position.X + (width/2), position.Y);
        set {
            position = new(value.X - (width/2), value.Y);
        }
    }

    public Point Bottom {
        get => new(position.X + (width/2), position.Y + height);
        set {
            position = new(value.X - (width/2), value.Y - height);
        }
    }

    public float AngleTo(Vector2 target)
	{
		return MathF.Atan2(target.Y - Center.Y, target.X - Center.X);
	}

    public float AngleTo(Point target)
	{
		return MathF.Atan2(target.Y - Center.Y, target.X - Center.X);
	}

	public bool WithinRange(Vector2 target, float maxRange)
	{
		return Vector2.DistanceSquared(Center.ToVector2(), target) <= maxRange * maxRange;
	}
}
