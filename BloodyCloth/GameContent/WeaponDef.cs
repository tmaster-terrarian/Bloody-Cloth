using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.GameContent;

public class WeaponDef : ContentDef
{
    public Texture2D IconTexture { get; set; }
}

public enum WeaponType
{
    Invalid = -1,
    Crossbow,
}
