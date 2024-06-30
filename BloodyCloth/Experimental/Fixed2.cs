using System;
using System.Runtime.Serialization;

using Microsoft.Xna.Framework;

namespace BloodyCloth.Experimental;

[DataContract]
public struct Fixed2 : IEquatable<Fixed2>
{
    [DataMember]
    public Fixed X;

    [DataMember]
    public Fixed Y;

    public readonly bool Equals(Fixed2 other)
    {
        return this.X == other.X && this.Y == other.Y;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Fixed2 fixed2 && Equals(fixed2);
    }

    public readonly Vector2 ToVector2()
    {
        return new((float)X, (float)Y);
    }

    public readonly Point ToPoint()
    {
        return new((int)X, (int)Y);
    }

    public static bool operator ==(Fixed2 left, Fixed2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Fixed2 left, Fixed2 right)
    {
        return !(left == right);
    }
}
