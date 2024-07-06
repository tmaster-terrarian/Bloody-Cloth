using System;

using Microsoft.Xna.Framework;

namespace BloodyCloth.GameContent;

public abstract class ContentDef
{
    public string Name { get; set; }

    public ContentDef()
    {
        Name = GetType().Name;
    }
}
