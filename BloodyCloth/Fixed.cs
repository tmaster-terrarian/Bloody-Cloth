using System;

namespace BloodyCloth;

public readonly struct Fixed : IEquatable<Fixed>
{
    private readonly int wholePart = 0;
    private readonly ushort decimalPart = 0;

    private readonly double DecimalPartToDouble => (double)decimalPart / 65536 * Math.Sign(wholePart);
    private readonly float DecimalPartToFloat => (float)decimalPart / 65536 * Math.Sign(wholePart);

    private Fixed(int wholePart, ushort decimalPart)
    {
        this.wholePart = wholePart;
        this.decimalPart = decimalPart;
    }

    public static Fixed From(float value)
    {
        int _int = (int)value;
        return new(_int, (ushort)(Math.Abs(value - _int) * 65536));
    }

    public static Fixed From(double value)
    {
        int _int = (int)value;
        return new(_int, (ushort)(Math.Abs(value - _int) * 65536));
    }

    public static explicit operator float(Fixed a)
    {
        return a.wholePart + a.DecimalPartToFloat;
    }

    public static explicit operator double(Fixed a)
    {
        return a.wholePart + a.DecimalPartToDouble;
    }

    public static explicit operator int(Fixed a)
    {
        return a.wholePart;
    }

    public static implicit operator Fixed(int a)
    {
        return new(a, 0);
    }

    public static Fixed operator +(Fixed a, int b)
    {
        return new(a.wholePart + b, a.decimalPart);
    }

    public static Fixed operator -(Fixed a, int b)
    {
        return a + -b;
    }

    public static Fixed operator +(Fixed a, float b)
    {
        return new(a.wholePart + (int)b, (ushort)(Math.Abs(b - (int)b) * 65536));
    }

    public static Fixed operator -(Fixed a, float b)
    {
        return a + -b;
    }

    public static Fixed operator +(Fixed a, double b)
    {
        return new(a.wholePart + (int)b, (ushort)(Math.Abs(b - (int)b) * 65536));
    }

    public static Fixed operator -(Fixed a, double b)
    {
        return a + -b;
    }

    public static Fixed operator +(Fixed a, Fixed b)
    {
        return new(a.wholePart + b.wholePart, (ushort)((Math.Sign(a.wholePart) * a.decimalPart + Math.Sign(b.wholePart) * b.decimalPart) % 65536));
    }

    public static Fixed operator -(Fixed a, Fixed b)
    {
        return a + -b;
    }

    public static Fixed operator -(Fixed value)
    {
        return new(-value.wholePart, value.decimalPart);
    }

    public static Fixed operator *(Fixed a, Fixed b)
    {
        return From((a.wholePart + a.DecimalPartToDouble) * (b.wholePart + b.DecimalPartToDouble));
    }

    public static Fixed operator /(Fixed a, Fixed b)
    {
        return From((a.wholePart + a.DecimalPartToDouble) / (b.wholePart + b.DecimalPartToDouble));
    }

    public static bool operator >(Fixed a, Fixed b)
    {
        return (a.wholePart + a.DecimalPartToDouble) > (b.wholePart + b.DecimalPartToDouble);
    }

    public static bool operator <(Fixed a, Fixed b)
    {
        return (a.wholePart + a.DecimalPartToDouble) < (b.wholePart + b.DecimalPartToDouble);
    }

    public static bool operator ==(Fixed a, Fixed b)
    {
        return a.wholePart == b.wholePart && a.decimalPart == b.decimalPart;
    }

    public static bool operator !=(Fixed a, Fixed b)
    {
        return a.wholePart != b.wholePart || a.decimalPart != b.decimalPart;
    }

    public readonly bool Equals(Fixed other)
    {
        return wholePart == other.wholePart && decimalPart == other.decimalPart;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Fixed @fixed && Equals(@fixed);
    }
}
