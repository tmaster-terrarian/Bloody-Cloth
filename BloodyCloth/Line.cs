using System;
using System.Runtime.Serialization;
using BloodyCloth.Utils;
using Microsoft.Xna.Framework;

namespace BloodyCloth;

[DataContract]
public struct Line : IEquatable<Line>
{
    [DataMember]
    public Point P1 { get; set; }

    [DataMember]
    public Point P2 { get; set; }

    public int Thickness { get; set; }

    public static Line Empty => new();

    public Point this[int index]
    {
        readonly get
        {
            return index switch
            {
                0 => P1,
                1 => P2,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }
        set
        {
            switch(index)
            {
                case 0: P1 = value; break;
                case 1: P2 = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    public Line(Point p1, Point p2, int thickness)
    {
        this.P1 = p1;
        this.P2 = p2;
        this.Thickness = thickness;
    }

    public Line(int x1, int y1, int x2, int y2, int thickness)
    {
        this.P1 = new(x1, y1);
        this.P2 = new(x2, y2);
        this.Thickness = thickness;
    }

    public readonly bool Intersects(Rectangle rectangle)
    {
        int minX = MathHelper.Min(P1.X, P2.X);
        int minY = MathHelper.Min(P1.Y, P2.Y);
        int maxX = MathHelper.Max(P1.X, P2.X);
        int maxY = MathHelper.Max(P1.Y, P2.Y);

        if(
            rectangle.X + rectangle.Width <= minX
            || rectangle.Y + rectangle.Height <= minY
            || rectangle.X >= maxX
            || rectangle.Y >= maxY
        ) return false;

        if(rectangle.Contains(P1) || rectangle.Contains(P2)) return true;

        Main.World.NumCollisionChecks++;

        int rx1;
        int rx2;

        if(rectangle.Top - maxY < 0)
        {
            if(P2.Y > P1.Y)
            {
                rx1 = rectangle.Left - minX;
                rx2 = rectangle.Right - minX;
            }
            else
            {
                rx1 = rectangle.Left - maxX;
                rx2 = rectangle.Right - maxX;
            }

            float y1 = (float)(P2.Y - P1.Y) / (P2.X - P1.X) * rx1;
            float y2 = (float)(P2.Y - P1.Y) / (P2.X - P1.X) * rx2;

            if(rectangle.Bottom - P1.Y < y1 && rectangle.Bottom - P2.Y > y2) return true;
            if(rectangle.Bottom - P1.Y > y1 && rectangle.Bottom - P2.Y < y2) return true;
        }


        return false;
    }

    public readonly bool Contains(Point point)
    {
        int val = MathUtil.Sqr(point.X - P1.X) + MathUtil.Sqr(point.Y - P1.Y) + MathUtil.Sqr(point.X - P2.X) + MathUtil.Sqr(point.Y - P2.Y);
        int len = MathUtil.Sqr(P1.X - P2.X) + MathUtil.Sqr(P1.Y - P1.Y);

        return val >= len - MathUtil.Sqr(Thickness / 2f) && val <= len + MathUtil.Sqr(Thickness / 2f);
    }

    public static bool operator ==(Line a, Line b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Line a, Line b)
    {
        return !a.Equals(b);
    }

    public readonly bool Equals(Line other)
    {
        return other.P1 == P1 && other.P2 == P2 && other.Thickness == Thickness;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Line triangle && Equals(triangle);
    }
}
