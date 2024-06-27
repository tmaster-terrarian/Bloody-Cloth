using System;
using System.Runtime.Serialization;

using Microsoft.Xna.Framework;

namespace BloodyCloth;

[DataContract]
public struct Triangle : IEquatable<Triangle>
{
    [DataMember]
    public Point P1 { get; set; }

    [DataMember]
    public Point P2 { get; set; }

    [DataMember]
    public Point P3 { get; set; }

    public static Triangle Empty => new();

    public Point this[int index]
    {
        readonly get
        {
            return index switch
            {
                0 => P1,
                1 => P2,
                2 => P3,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }
        set
        {
            switch(index)
            {
                case 0: P1 = value; break;
                case 1: P2 = value; break;
                case 2: P3 = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    public Triangle(Point p1, Point p2, Point p3)
    {
        this.P1 = p1;
        this.P2 = p2;
        this.P3 = p3;
    }

    public Triangle(int x1, int y1, int x2, int y2, int x3, int y3)
    {
        this.P1 = new(x1, y1);
        this.P2 = new(x2, y2);
        this.P3 = new(x3, y3);
    }

    public readonly bool Intersects(Rectangle rectangle)
    {
        for(int x = 0; x < rectangle.Width; x++)
        {
            for(int y = 0; y < rectangle.Height; y++)
            {
                if(Contains(new Point(x, y) + rectangle.Location))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public readonly bool Contains(Point point)
    {
        int d1, d2, d3;
        bool has_neg, has_pos;
        d1 = Sign(new(point, P1, P2));
        d2 = Sign(new(point, P2, P3));
        d3 = Sign(new(point, P3, P1));
        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(has_neg && has_pos);
    }

    public static int Sign(Triangle triangle)
    {
        return (triangle.P1.X - triangle.P3.X) * (triangle.P2.Y - triangle.P3.Y) - (triangle.P2.X - triangle.P3.X) * (triangle.P1.Y - triangle.P3.Y);
    }

    public readonly int Sign()
    {
        return Sign(this);
    }

    public static bool operator ==(Triangle a, Triangle b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Triangle a, Triangle b)
    {
        return !a.Equals(b);
    }

    public readonly bool Equals(Triangle other)
    {
        return other.P1 == P1 && other.P2 == P2 && other.P3 == P3;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Triangle triangle && Equals(triangle);
    }
}
