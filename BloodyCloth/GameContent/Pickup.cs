using System;

namespace BloodyCloth.GameContent;

public class Pickup : Entity
{
    private static readonly Pickup[] pickups = new Pickup[200];
    private static uint _pickupID;

    private readonly WeaponType weaponDefId;
    private readonly SpellType spellDefId;
    private bool markedForRemoval;

    public uint ID { get; private set; }

    public string TexturePath { get; set; }

    public WeaponType WeaponDefID => weaponDefId;
    public SpellType SpellDefID => spellDefId;

    public bool HasValidWeaponType => weaponDefId >= 0;
    public bool HasValidSpellType => spellDefId >= 0;

    private Pickup()
    {
        this.ID = _pickupID++;
        this.weaponDefId = WeaponType.Invalid;
    }

    private Pickup(WeaponType type) : this()
    {
        if(type >= 0)
            this.weaponDefId = type;
    }

    private Pickup(SpellType type) : this()
    {
        if(type >= 0)
            this.spellDefId = type;
    }
}
