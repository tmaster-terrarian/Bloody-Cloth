using System;

using Microsoft.Xna.Framework;

using BloodyCloth.Utils;

namespace BloodyCloth;

public abstract class Entity
{
	// alright lets not make it stupid complicated this time

	int _layerDepth = 0;

	protected Point position;
	protected Vector2 drawScale = Vector2.One;

	public Vector2 velocity;

	public float LayerDepth
	{
		get => 1 - ((float)(_layerDepth + 10000) / 20000);
		set => _layerDepth = Extensions.Floor((1 - MathHelper.Clamp(value, 0, 1)) * 20000 - 10000);
	}

	public Color Color { get; set; } = Color.White;

	public int Width { get; protected set; } = 8;
	public int Height { get; protected set; } = 8;

	public Vector2 DrawScale => drawScale;

	public virtual bool Active { get; private set; } = true;

	public virtual bool NoCollide { get; set; }
	public virtual bool CollidesWithSolids { get; set; } = true;
	public virtual bool CollidesWithJumpthroughs { get; set; } = true;

	public bool OnGround { get; protected set; }

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

	public bool CheckOnGround()
	{
		return CheckColliding(Hitbox.Shift(0, 1));
	}

	public bool CheckColliding(Rectangle rectangle, bool ignoreJumpThroughs = false)
	{
		if(NoCollide) return false;

		if(Main.World.TileMeeting(rectangle)) return true;
		if(CollidesWithSolids && Main.World.SolidMeeting(rectangle)) return true;

		if(!ignoreJumpThroughs)
		{
			return CheckCollidingJumpthrough(rectangle);
		}

		return false;
	}

	public bool CheckCollidingJumpthrough(Rectangle rectangle)
	{
		if(NoCollide) return false;
		if(!CollidesWithJumpthroughs) return false;

		// I bet you 100% that this is really laggy in large scale usage lmao

		Rectangle newRect = new(rectangle.Left, rectangle.Bottom - 1, rectangle.Width, 1);

		Rectangle rect = Main.World.JumpThroughPlace(newRect) ?? Rectangle.Empty;
		Rectangle rect2 = Main.World.JumpThroughPlace(newRect.Shift(0, -1)) ?? Rectangle.Empty;

		if(rect != Rectangle.Empty) return rect != rect2;

		Line tri = Main.World.JumpThroughSlopePlace(newRect) ?? Line.Empty;
		Line tri2 = Main.World.JumpThroughSlopePlace(newRect.Shift(0, -1)) ?? Line.Empty;

		if(tri != Line.Empty) return tri != tri2;

		return false;
	}

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
        if(float.IsNaN(vector2.X) || float.IsInfinity(vector2.X) || float.IsSubnormal(vector2.X)) velocity.X = 0;
        if(float.IsNaN(vector2.Y) || float.IsInfinity(vector2.Y) || float.IsSubnormal(vector2.Y)) velocity.Y = 0;

		return vector2;
	}

	public Vector2 DirectionTo(Point target)
	{
		Vector2 vector2 = Vector2.Normalize(target.ToVector2() - Center.ToVector2());
        if(float.IsNaN(vector2.X) || float.IsInfinity(vector2.X) || float.IsSubnormal(vector2.X)) velocity.X = 0;
        if(float.IsNaN(vector2.Y) || float.IsInfinity(vector2.Y) || float.IsSubnormal(vector2.Y)) velocity.Y = 0;

		return vector2;
	}

	public Vector2 DirectionFrom(Vector2 target)
	{
		Vector2 vector2 = Vector2.Normalize(Center.ToVector2() - target);
        if(float.IsNaN(vector2.X) || float.IsInfinity(vector2.X) || float.IsSubnormal(vector2.X)) velocity.X = 0;
        if(float.IsNaN(vector2.Y) || float.IsInfinity(vector2.Y) || float.IsSubnormal(vector2.Y)) velocity.Y = 0;

		return vector2;
	}

	public Vector2 DirectionFrom(Point target)
	{
		Vector2 vector2 = Vector2.Normalize(Center.ToVector2() - target.ToVector2());
        if(float.IsNaN(vector2.X) || float.IsInfinity(vector2.X) || float.IsSubnormal(vector2.X)) velocity.X = 0;
        if(float.IsNaN(vector2.Y) || float.IsInfinity(vector2.Y) || float.IsSubnormal(vector2.Y)) velocity.Y = 0;

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
