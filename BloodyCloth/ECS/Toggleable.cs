using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs;

public abstract class Toggleable
{
    private bool _enabled = true;

    public virtual bool IsEnabled { get => _enabled; protected set => _enabled = value; }

    public virtual void OnEnable() {}

    public virtual void OnDisable() {}

    public void SetEnabled(bool value)
    {
        if(IsEnabled != value)
        {
            IsEnabled = value;
            if(value) this.OnEnable();
            else this.OnDisable();
        }
    }
}
