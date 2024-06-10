using Microsoft.Xna.Framework;

namespace BloodyCloth;

public abstract class AbstractTile
{
    public string id;
    public byte shape;
    public float breakingProgress;
}

public class Tile : AbstractTile
{
    // 0000 : LBRT

    private static readonly Rectangle[] UVshapes = {
        new Rectangle(0, 0, 1, 1),   // 00 0000
        new Rectangle(0, 3, 1, 1),   // 01 0001
        new Rectangle(1, 0, 1, 1),   // 02 0010
        new Rectangle(1, 3, 1, 1),   // 03 0011
        new Rectangle(0, 1, 1, 1),   // 04 0100
        new Rectangle(0, 2, 1, 1),   // 05 0101
        new Rectangle(1, 1, 1, 1),   // 06 0110
        new Rectangle(1, 2, 1, 1),   // 07 0111
        new Rectangle(3, 0, 1, 1),   // 08 1000
        new Rectangle(3, 3, 1, 1),   // 09 1001
        new Rectangle(2, 0, 1, 1),   // 10 1010
        new Rectangle(2, 3, 1, 1),   // 11 1011
        new Rectangle(3, 1, 1, 1),   // 12 1100
        new Rectangle(3, 2, 1, 1),   // 13 1101
        new Rectangle(2, 1, 1, 1),   // 14 1110
        new Rectangle(2, 2, 1, 1),   // 15 1111
    };

    public Tile()
    {
        this.id = "air";
        this.shape = 0;
    }

    public Tile(string id, byte shape = 0)
    {
        this.id = id;
        this.shape = shape;
    }

    public override string ToString()
    {
        return $"Tile {id}";
    }

    public static Rectangle GetShapeUV(byte shape)
    {
        if(shape < 16 && shape >= 0) return UVshapes[shape];
        else return UVshapes[0];
    }
}
