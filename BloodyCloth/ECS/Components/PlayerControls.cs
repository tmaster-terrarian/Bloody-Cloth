using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BloodyCloth.Ecs.Components
{
    [System.Serializable]
    public class PlayerControls : Component
    {
        public class PlayerInputMapping
        {
            public Keys Right { get; set; } = Keys.D;
            public Keys Left { get; set; } = Keys.A;
            public Keys Down { get; set; } = Keys.S;
            public Keys Up { get; set; } = Keys.W;
            public Keys Jump { get; set; } = Keys.Space;
        }

        public PlayerIndex GamePadIndex { get; set; }

        public PlayerInputMapping inputMapping = new PlayerInputMapping {
            // rebind here with Name = Keys
        };

        public PlayerControls()
        {
            Systems.PlayerControlsSystem.Register(this);
        }

        public override void Update()
        {
            

            base.Update();
        }
    }
}

namespace BloodyCloth.Ecs.Systems { public class PlayerControlsSystem : ComponentSystem<Components.PlayerControls> {} }
