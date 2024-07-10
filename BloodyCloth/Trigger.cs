using System.Collections.Generic;

using Microsoft.Xna.Framework;

using BloodyCloth.GameContent;
using Microsoft.Xna.Framework.Graphics;
using BloodyCloth.Graphics;

namespace BloodyCloth;

public class Trigger
{
    private static List<Trigger> triggers = [];

    private TriggerType defId = TriggerType.Invalid;
    private bool markedForRemoval;
    private bool wasTouchingPlayer;

    public Rectangle Bounds { get; set; }

    public TriggerType DefID => defId;

    public bool HasValidType => DefID >= 0;

    private Trigger(TriggerType type, Rectangle bounds)
    {
        Bounds = bounds;

        if(type != TriggerType.Invalid) 
            defId = type;
    }

    public static void Create(TriggerType type, Rectangle bounds)
    {
        triggers.Add(new(type, bounds));
    }

    public static void Update()
    {
        for(int i = 0; i < triggers.Count; i++)
        {
            Trigger trigger = triggers[i];

            if(trigger.markedForRemoval)
            {
                triggers.RemoveAt(i);
                i--;
                continue;
            }

            if(trigger.HasValidType)
            {
                TriggerDef def = Defs.TriggerDefs[trigger.DefID];

                bool touching = Main.Player.Hitbox.Intersects(trigger.Bounds);

                if(!trigger.wasTouchingPlayer && touching)
                {
                    def.OnEnter(trigger);
                    if(def.TriggerOnce) trigger.markedForRemoval = true;
                }

                if(touching && !def.TriggerOnce)
                {
                    def.OnStay(trigger);
                }

                if(trigger.wasTouchingPlayer && !touching)
                {
                    def.OnExit(trigger);
                }

                trigger.wasTouchingPlayer = touching;
            }
        }
    }

    public static void Draw()
    {
        if(!Main.Debug.Enabled) return;

        Texture2D tex = Main.LoadContent<Texture2D>("Images/Other/tileOutline");

        for(int i = 0; i < triggers.Count; i++)
        {
            Trigger trigger = triggers[i];

            if(trigger.markedForRemoval) continue;

            NineSlice.DrawNineSlice(
                tex,
                trigger.Bounds,
                null,
                new Point(1),
                new Point(1),
                Color.LightPink * 0.5f,
                Vector2.Zero,
                SpriteEffects.None,
                0
            );

            Renderer.SpriteBatch.Base.DrawStringSpacesFix(Renderer.SmallFont, /*nameof(TriggerType) + "." +*/ trigger.DefID.ToString(), trigger.Bounds.Location.ToVector2() + new Vector2(2, -1), Color.White * 0.9f, 2);
        }
    }

    public static void ClearAll()
    {
        triggers = [];
    }
}
