using System;

namespace BloodyCloth.Experimental;

public readonly struct Fixed : IEquatable<Fixed>
{
    private readonly ulong packedValue = 0;

    private readonly uint WholePart => (uint)packedValue >> 16;
    private readonly ushort DecimalPart => (ushort)(packedValue & ushort.MaxValue);

    private readonly bool sign = false;

    private readonly int SignValue => sign ? -1 : 1;

    private readonly double DecimalPartToDouble => (double)DecimalPart / (ushort.MaxValue + 1);

    private Fixed(int wholePart, ushort decimalPart, bool sign = false)
    {
        this.packedValue = decimalPart | ((uint)wholePart << 16);
        this.sign = sign;
    }

    private Fixed(uint wholePart, ushort decimalPart, bool sign = false)
    {
        this.packedValue = decimalPart | (wholePart << 16);
        this.sign = sign;
    }

    public static Fixed From(float value)
    {
        int _int = (int)value;
        return new(_int, (ushort)(Math.Abs(value - _int) * (ushort.MaxValue + 1)), Math.Sign(value) == -1);
    }

    public static Fixed From(double value)
    {
        int _int = (int)value;
        return new(_int, (ushort)(Math.Abs(value - _int) * (ushort.MaxValue + 1)), Math.Sign(value) == -1);
    }

    public static explicit operator float(Fixed value)
    {
        return (value.WholePart + (float)value.DecimalPartToDouble) * value.SignValue;
    }

    public static explicit operator double(Fixed value)
    {
        return (value.WholePart + value.DecimalPartToDouble) * value.SignValue;
    }

    public static explicit operator int(Fixed value)
    {
        return (int)value.WholePart * value.SignValue;
    }

    public static implicit operator Fixed(int value)
    {
        return new(Math.Abs(value), 0, Math.Sign(value) == -1);
    }

    public static Fixed operator +(Fixed a, int b)
    {
        int sign = Math.Sign(a.WholePart + b);
        return new(Math.Abs((int)a.WholePart + b), a.DecimalPart, sign == -1);
    }

    public static Fixed operator -(Fixed a, int b)
    {
        int sign = Math.Sign(a.WholePart + b);
        return new(Math.Abs((int)a.WholePart - b), a.DecimalPart, sign == -1);
    }

    public static Fixed operator +(Fixed a, Fixed b)
    {
        return new(a.WholePart + b.WholePart, (ushort)((a.DecimalPart + b.DecimalPart) % (ushort.MaxValue + 1)));
    }

    public static Fixed operator -(Fixed a, Fixed b)
    {
        return a + -b;
    }

    public static Fixed operator -(Fixed value)
    {
        return new(value.WholePart, value.DecimalPart, !value.sign);
    }

    public static Fixed operator *(Fixed a, int b)
    {
        return From((a.WholePart + a.DecimalPartToDouble) * b);
    }

    public static Fixed operator /(Fixed a, int b)
    {
        return From((a.WholePart + a.DecimalPartToDouble) / b);
    }

    public static Fixed operator *(Fixed a, Fixed b)
    {
        return From((a.WholePart + a.DecimalPartToDouble) * (b.WholePart + b.DecimalPartToDouble));
    }

    public static Fixed operator /(Fixed a, Fixed b)
    {
        return From((a.WholePart + a.DecimalPartToDouble) / (b.WholePart + b.DecimalPartToDouble));
    }

    public static bool operator >(Fixed a, Fixed b)
    {
        if(a.sign && !b.sign) return true;
        if(!a.sign && b.sign) return false;

        ulong value1 = a.DecimalPart | (a.WholePart << 16);
        ulong value2 = b.DecimalPart | (b.WholePart << 16);

        return value1 > value2;
    }

    public static bool operator <(Fixed a, Fixed b)
    {
        if(a.sign && !b.sign) return false;
        if(!a.sign && b.sign) return true;

        ulong value1 = a.DecimalPart | (a.WholePart << 16);
        ulong value2 = b.DecimalPart | (b.WholePart << 16);

        return value1 < value2;
    }

    public static bool operator ==(Fixed a, Fixed b)
    {
        return ((a.DecimalPart | ((ulong)a.WholePart << 16)) == (b.DecimalPart | ((ulong)b.WholePart << 16))) && a.sign == b.sign;
    }

    public static bool operator !=(Fixed a, Fixed b)
    {
        return ((a.DecimalPart | ((ulong)a.WholePart << 16)) != (b.DecimalPart | ((ulong)b.WholePart << 16))) || a.sign != b.sign;
    }

    public readonly bool Equals(Fixed other)
    {
        return WholePart == other.WholePart && DecimalPart == other.DecimalPart && sign == other.sign;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Fixed @fixed && Equals(@fixed);
    }
}
