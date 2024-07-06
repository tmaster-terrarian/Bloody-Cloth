using System;

using Microsoft.Xna.Framework;

namespace BloodyCloth;

public abstract class Entity
{
	// alright lets not make it stupid complicated this time

	int _depth = 0;

	protected Point position;
	protected Vector2 drawScale = Vector2.One;

	public Vector2 velocity;

	/// <summary>
	/// <para>Integer layer depth of the entity.</para>
	/// <para>Use <see cref="ConvertedLayerDepth"/> for drawing.</para>
	/// </summary>
	public int LayerDepth
	{
		get => _depth;
		set => _depth = MathHelper.Clamp(value, -100000, 100000);
	}

	/// <summary>
	/// Layer depth of the entity, converted to work with drawing.
	/// </summary>
    public float ConvertedLayerDepth => 1 - ((_depth + 100000) * 5e-6f);

    public Color Color { get; set; } = Color.White;

	public int Width { get; protected set; } = 8;
	public int Height { get; protected set; } = 8;

	public Vector2 DrawScale => drawScale;

	public virtual bool Active { get; protected set; } = true;

	public float Rotation { get; set; }

	public int Facing { get; set; } = 1;

	public Point Center {
		get => new(position.X + (Width/2), position.Y + (Height/2));
		set {
			position = new(value.X - (Width/2), value.Y - (Height/2));
		}
	}

	public Point TopLeft {
		get => position;
		set {
			position = value;
		}
	}

	public Point TopRight {
		get => new(position.X + Width, position.Y);
		set {
			position = new(value.X - Width, value.Y);
		}
	}

	public Point BottomLeft {
		get => new(position.X, position.Y + Height);
		set {
			position = new(value.X, value.Y - Height);
		}
	}

	public Point BottomRight {
		get => new(position.X + Width, position.Y + Height);
		set {
			position = new(value.X - Width, value.Y - Height);
		}
	}

	public Rectangle Hitbox {
		get => new(position.X, position.Y, Width, Height);
		set {
			position = value.Location;
			Width = value.Width;
			Height = value.Height;
		}
	}

	public Rectangle RightEdge => new(Right.X - 1, Top.Y, 1, Height);

	public Rectangle LeftEdge => new(Left.X, Top.Y, 1, Height);

	public Rectangle TopEdge => new(Left.X, Top.Y, Width, 1);

	public Rectangle BottomEdge => new(Left.X, Bottom.Y - 1, Width, 1);

	public Point Left {
		get => new(position.X, position.Y + (Height/2));
		set {
			position = new(value.X, value.Y - (Height/2));
		}
	}

	public Point Right {
		get => new(position.X + Width, position.Y + (Height/2));
		set {
			position = new(value.X - Width, value.Y - (Height/2));
		}
	}

	public Point Top {
		get => new(position.X + (Width/2), position.Y);
		set {
			position = new(value.X - (Width/2), value.Y);
		}
	}

	public Point Bottom {
		get => new(position.X + (Width/2), position.Y + Height);
		set {
			position = new(value.X - (Width/2), value.Y - Height);
		}
	}

	/// <summary>
	/// Converts an entity depth value to a value that works for drawing.
	/// </summary>
	/// <param name="depth"></param>
	/// <returns></returns>
	public static float ConvertDepth(int depth) => 1 - ((MathHelper.Clamp(depth, -100000, 100000) + 100000) * 5e-6f);

	public float AngleTo(Vector2 target)
	{
		return MathF.Atan2(target.Y - Center.Y, target.X - Center.X);
	}

	public float AngleTo(Point target)
	{
		return MathF.Atan2(target.Y - Center.Y, target.X - Center.X);
	}

	public float AngleFrom(Vector2 target)
	{
		return MathF.Atan2(Center.Y - target.Y, Center.X - target.X);
	}

	public float AngleFrom(Point target)
	{
		return MathF.Atan2(Center.Y - target.Y, Center.X - target.X);
	}

	public Vector2 DirectionTo(Vector2 target)
	{
		Vector2 vector2 = Vector2.Normalize(target - Center.ToVector2());
        if(float.IsNaN(vector2.X) || float.IsInfinity(vector2.X) || float.IsSubnormal(vector2.X)) vector2.X = 0;
        if(float.IsNaN(vector2.Y) || float.IsInfinity(vector2.Y) || float.IsSubnormal(vector2.Y)) vector2.Y = 0;

		return vector2;
	}

	public Vector2 DirectionTo(Point target)
	{
		Vector2 vector2 = Vector2.Normalize(target.ToVector2() - Center.ToVector2());
        if(float.IsNaN(vector2.X) || float.IsInfinity(vector2.X) || float.IsSubnormal(vector2.X)) vector2.X = 0;
        if(float.IsNaN(vector2.Y) || float.IsInfinity(vector2.Y) || float.IsSubnormal(vector2.Y)) vector2.Y = 0;

		return vector2;
	}

	public Vector2 DirectionFrom(Vector2 target)
	{
		Vector2 vector2 = Vector2.Normalize(Center.ToVector2() - target);
        if(float.IsNaN(vector2.X) || float.IsInfinity(vector2.X) || float.IsSubnormal(vector2.X)) vector2.X = 0;
        if(float.IsNaN(vector2.Y) || float.IsInfinity(vector2.Y) || float.IsSubnormal(vector2.Y)) vector2.Y = 0;

		return vector2;
	}

	public Vector2 DirectionFrom(Point target)
	{
		Vector2 vector2 = Vector2.Normalize(Center.ToVector2() - target.ToVector2());
        if(float.IsNaN(vector2.X) || float.IsInfinity(vector2.X) || float.IsSubnormal(vector2.X)) vector2.X = 0;
        if(float.IsNaN(vector2.Y) || float.IsInfinity(vector2.Y) || float.IsSubnormal(vector2.Y)) vector2.Y = 0;

		return vector2;
	}

	public float DistanceSquaredTo(Vector2 target)
	{
		return Vector2.DistanceSquared(Center.ToVector2(), target);
	}

	public float DistanceSquaredTo(Point target)
	{
		return Vector2.DistanceSquared(Center.ToVector2(), target.ToVector2());
	}

	public int TaxicabDistanceTo(Point target)
	{
		return Math.Abs(target.X - Center.X) + Math.Abs(target.Y - Center.Y);
	}

	public float ChebyshevDistanceTo(Vector2 target)
	{
		return MathHelper.Max(Math.Abs(target.X - Center.X), Math.Abs(target.Y - Center.Y));
	}

	public int ChebyshevDistanceTo(Point target)
	{
		return MathHelper.Max(Math.Abs(target.X - Center.X), Math.Abs(target.Y - Center.Y));
	}

	public bool WithinRange(Vector2 target, float maxRange)
	{
		return Vector2.DistanceSquared(Center.ToVector2(), target) <= maxRange * maxRange;
	}

	public bool WithinRange(Point target, float maxRange)
	{
		return Vector2.DistanceSquared(Center.ToVector2(), target.ToVector2()) <= maxRange * maxRange;
	}
}
