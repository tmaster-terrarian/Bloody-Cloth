using System;

using Microsoft.Xna.Framework;

namespace BloodyCloth.GameContent;

public abstract class ContentDef<T> where T : Enum
{
    public string Name { get; set; }
}
