using BloodyCloth.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.Experimental;

public class VerletRope
{
    private readonly Entity target;
    private readonly int segments;

    private readonly Texture2D texture = Main.LoadContent<Texture2D>("Images/Other/circle");

    private readonly Vector2[] points;

    public Vector2 position = Vector2.Zero;
    public Vector2 offset = Vector2.Zero;

    public Color StartColor { get; set; } = Color.White;

    public Color EndColor { get; set; } = Color.White;

    public VerletRope(Entity target, int segments)
    {
        this.target = target;

        this.segments = segments;

        points = new Vector2[segments];

        points[0] = new(
            target.position.X + target.velocity.X,
            target.position.Y + target.velocity.Y + 2
        );
    }

    public void Update()
    {
        points[0] = new(
            position.X + target.velocity.X,
            position.Y + target.velocity.Y + 2
        );

        float size = 3.2f;

        float ds_ = size / 3.2f;
        float ns_ = 3.5f * ds_;

        float grv = 1;

        Vector2 offs = new(
            (target.Facing * 0.5f + ( 1 - grv ) * 2.3f) * ds_,
            2.9f * grv * ds_
        );

        float pwr_ = 0.5f * grv * ds_;

        for(int i = 0; i < segments - 1; i++)
        {
            ref var h = ref points[i + 1];
            ref var h2 = ref points[i];

            h = Vector2.Lerp(h, new(h2.X - offs.X, h2.Y + offs.Y), pwr_);

            if(Vector2.DistanceSquared(h, h2) > ns_*ns_)
            {
                h = h2 - (Vector2.Normalize(h2 - h) * ns_);
            }
        }
    }

    public void Draw()
    {
        float ll_ = segments * 1.5f;

        for(int i = 0; i < segments; i++)
        {
            var c_ = Color.Lerp(StartColor, EndColor, i/ll_);
            Renderer.SpriteBatch.Draw(texture, points[i] - Vector2.One * 4 - offset, c_);
        }
    }
}
