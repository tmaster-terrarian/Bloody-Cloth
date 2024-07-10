using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Graphics;

namespace BloodyCloth.GameContent;

public class TriggerDef : AbstractDef
{
    public bool TriggerOnce { get; set; }

    public Action<Trigger> OnEnter { get; set; }
    public Action<Trigger> OnExit { get; set; }
    public Action<Trigger> OnStay { get; set; }
}

public enum TriggerType
{
    Invalid = -1,
    NextRoom,
}
