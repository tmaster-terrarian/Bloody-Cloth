// Source: https://raw.githubusercontent.com/IrishBruse/LDtkMonogame/main/LDtk.LevelViewer/Camera.cs
// License: MIT

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth;

public class Camera
{
    readonly GraphicsDevice graphicsDevice;

    public Vector2 Position { get; set; } = Vector2.Zero;

    public float Zoom { get; set; } = 1;

    public Matrix Transform { get; private set; } = new();

    public Camera(GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
    }

    public void Update()
    {
        Transform = Matrix.CreateTranslation(new Vector3(Extensions.Round(-Position.X), Extensions.Round(-Position.Y), 0)) * Matrix.CreateScale(Zoom);
    }
}
