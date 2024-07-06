using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth.GameContent;

public interface IDealsDamageContentDef
{
    public int Damage { get; set; }
    public bool CanHurtPlayer { get; set; }
}
