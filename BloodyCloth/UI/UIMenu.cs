using Iguina;

namespace BloodyCloth.UI;

public abstract class UIMenu
{
    Iguina.Entities.Entity currentPage;

    public Iguina.Entities.Entity CurrentPage => currentPage;

    public UISystem UISystem { get; }
    public Iguina.Entities.Entity Root { get; }

    public bool PauseWhileOpen { get; set; }

    public UIMenu()
    {
        UISystem = Main.UISystem;
        Root = new Iguina.Entities.Entity(UISystem, null);

        CreateSelf();
    }

    protected virtual void CreateSelf()
    {
        Main.UISystem.Root.AddChild(Root);
    }

    public virtual void Destroy()
    {
        Root.RemoveSelf();
    }

    public virtual void HandleBackButton()
    {
        if(Main.ActiveMenu == this) Main.SetMenu(null);
    }

    public virtual void Update() {}

    public virtual void PostDraw() {}

    public virtual void PreDraw() {}

    protected void SetPage(Iguina.Entities.Entity page)
    {
        if(currentPage is not null)
        {
            if(!currentPage.IsCurrentlyLocked())
                currentPage.Locked = true;

            currentPage.RemoveSelf();
        }

        currentPage = page;

        if(page is not null)
            Root.AddChild(currentPage);
    }

    protected void SetPage<T>(EntityBuilder<T> builder) where T : Iguina.Entities.Entity => SetPage(builder.Build());
}
