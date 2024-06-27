using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.Ecs.Components
{
    [System.Serializable]
    public class Sprite : Component
    {
        int _layerDepth = 0;

        public Texture2D? texture = null;
        public Rectangle? sourceRectangle = null;
        public Point origin = Point.Zero;
        public Color color = Color.White;
        public SpriteEffects spriteEffects = SpriteEffects.None;

        public float LayerDepth
        {
            get => 1 - ((float)(_layerDepth + 10000) / 20000);
            set => _layerDepth = Extensions.Floor((1 - MathHelper.Clamp(value, 0, 1)) * 20000 - 10000);
        }

        public Sprite()
        {
            SpriteSystem.Register(this);
        }

        public override void Draw()
        {
            if(texture == null) return;

            Transform transform = Entity.GetComponent<Transform>();

            if(transform == null) return;

            Main.World.DrawSprite(this, transform);
        }

        protected override void CleanupUnmanaged()
        {
            texture.Dispose();
            texture = null;
        }
    }
}
