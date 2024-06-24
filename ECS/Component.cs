using System;
using Microsoft.Xna.Framework;

namespace BloodyCloth.Ecs;

public abstract class Component : Toggleable, IDisposable
{
    public Entity Entity { get; set; }

    public override bool IsEnabled { get => base.IsEnabled && (Entity?.IsEnabled ?? false); protected set => base.IsEnabled = value; }

    private bool _disposed = false;

    public bool IsDisposed => _disposed;

    public virtual void Update() {}

    public virtual void Draw() {}

    public virtual void OnCreate() {}

    public virtual void OnDestroy() {}

    ~Component()
    {
        Dispose(false);
    }

    protected virtual void CleanupManaged() {}

    protected virtual void CleanupUnmanaged() {}

    private void Dispose(bool disposing)
    {
        if(!_disposed)
        {
            if(disposing)
            {
                this.CleanupManaged();
            }

            this.Entity = null;
            this.CleanupUnmanaged();

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
