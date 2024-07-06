using System;

using BloodyCloth.GameContent;

namespace BloodyCloth;

public class Pickup : MoveableEntity
{
    private static readonly Pickup[] pickups = new Pickup[100];
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

    public static void Update()
    {
        for(int i = 0; i < pickups.Length; i++)
        {
            Pickup pickup = pickups[i];

            if(pickup is null) continue;

            pickup.OnGround = pickup.CheckOnGround();

            pickup.MoveX(pickup.velocity.X,
                () => {
                    pickup.velocity.X = 0;
                }
            );
            pickup.MoveY(pickup.velocity.Y,
                () => {
                    pickup.velocity.Y = 0;
                }
            );

            if(pickup.markedForRemoval)
            {
                pickups[i] = null;
            }
        }
    }

    public void Kill()
    {
        this.markedForRemoval = true;
    }
}
