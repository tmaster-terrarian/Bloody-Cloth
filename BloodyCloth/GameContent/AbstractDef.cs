using System;

using Microsoft.Xna.Framework;

namespace BloodyCloth.GameContent;

public abstract class AbstractDef
{
    public string Name { get; set; }

    public AbstractDef()
    {
        Name = GetType().Name;
    }
}
