using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Graphics;

namespace BloodyCloth.GameContent;

public abstract class EnemyDef : AbstractDef
{
    public int MaxHP { get; set; } = 1;
    public bool Invincible { get; set; }
    public bool RegeneratesHP { get; set; }

    public virtual void OnCreate(Enemy enemy)
    {
        enemy.MaxHP = MaxHP;
        enemy.Invincible = Invincible;
        enemy.RegeneratesHP = RegeneratesHP;
    }

    public virtual void OnStateEnter(Enemy enemy, EnemyState state) {}

    public virtual void OnStateExit(Enemy enemy, EnemyState state) {}

    public virtual void Update(Enemy enemy) {}

    public virtual void Draw(Enemy enemy) {}

    public virtual void OnDestroy(Enemy enemy) {}
}

public enum EnemyType
{
    Invalid = -1,
    Dummy
}
