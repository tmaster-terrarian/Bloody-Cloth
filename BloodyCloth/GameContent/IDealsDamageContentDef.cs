using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.GameContent;

public interface IDealsDamageContentDef
{
    public int Damage { get; }
    public bool CanHurtPlayer { get; }
    public bool CanHurtEnemy { get; }
}
