using Iguina;

namespace BloodyCloth.UI;

public abstract class UIMenu
{
    public UISystem UISystem { get; }
    public Iguina.Entities.Entity Root { get; }

    public bool PauseWhileOpen { get; set; }

    public UIMenu()
    {
        UISystem = Main.UISystem;
        Root = new Iguina.Entities.Entity(UISystem, null);

        CreateSelf();
    }

    public virtual void CreateSelf()
    {
        Main.UISystem.Root.AddChild(Root);
    }

    public virtual void Destroy()
    {
        Root.RemoveSelf();
    }

    public virtual void Update() {}

    public virtual void PostDraw() {}

    public virtual void PreDraw() {}
}
