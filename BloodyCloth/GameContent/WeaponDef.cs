using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.GameContent;

public class WeaponDef : AbstractDef, IDealsDamageContentDef
{
    public string IconTexturePath { get; set; }

    public int Damage { get; set; } = 1;
    public bool CanHurtPlayer { get; set; } = false;
    public bool CanHurtEnemy { get; set; } = true;

    public virtual void OnUse(Entity user)
    {
        
    }
}

public enum WeaponType
{
    Invalid = -1,
    Crossbow,
}
