using Microsoft.Xna.Framework;

namespace BloodyCloth.Utils;

public static class RectangleExtensions
{
    public static Rectangle Shift(this Rectangle rectangle, Point offset)
    {
        rectangle.X += offset.X;
        rectangle.Y += offset.Y;
        return rectangle;
    }

    public static Rectangle ScalePosition(this Rectangle rectangle, Point scale)
    {
        rectangle.X *= scale.X;
        rectangle.Y *= scale.Y;
        rectangle.Width *= scale.X;
        rectangle.Height *= scale.Y;
        return rectangle;
    }

    public static Rectangle ScalePosition(this Rectangle rectangle, Vector2 scale)
    {
        rectangle.X = (int)(rectangle.X * scale.X);
        rectangle.Y = (int)(rectangle.Y * scale.Y);
        rectangle.Width = (int)(rectangle.Width * scale.X);
        rectangle.Height = (int)(rectangle.Height * scale.Y);
        return rectangle;
    }

    public static Rectangle ScalePosition(this Rectangle rectangle, int scale) => ScalePosition(rectangle, new Point(scale, scale));
    public static Rectangle ScalePosition(this Rectangle rectangle, float scale) => ScalePosition(rectangle, Vector2.One * scale);
}
