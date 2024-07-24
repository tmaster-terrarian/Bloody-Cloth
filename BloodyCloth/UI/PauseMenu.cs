using Microsoft.Xna.Framework;

using BloodyCloth.Graphics;

using Iguina.Entities;

namespace BloodyCloth.UI;

public class PauseMenu : UIMenu
{
    Iguina.Entities.Entity currentPage;

    public Iguina.Entities.Entity CurrentPage => currentPage;

    public override void CreateSelf()
    {
        PauseWhileOpen = true;

        CreatePauseMenu();

        base.CreateSelf();
    }

    public override void HandleBackButton()
    {
        switch(currentPage.Identifier)
        {
            case "Settings":
                CreatePauseMenu();
                return;
            default:
                base.HandleBackButton();
                return;
        }
    }

    public override void PreDraw()
    {
        Renderer.SpriteBatch.Draw(Main.OnePixel, new Rectangle(Point.Zero, Main.ScreenSize), Color.Black * 0.5f);
    }

    private void SetPage(Iguina.Entities.Entity page)
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

    private void SetPage<T>(EntityBuilder<T> builder) where T : Iguina.Entities.Entity => SetPage(builder.Build());

    private void CreatePauseMenu()
    {
        SetPage(
            new Panel(UISystem) {
                Anchor = Iguina.Defs.Anchor.Center,
                AutoHeight = true,
                Identifier = "Root"
            }
            .Builder()
            .SetSizeInPixels(144, 24 * 3 + 8)
            .AddChild(
                new Button(UISystem, "Resume").Builder()
                .SetEventListener(EntityEventType.OnClick, entity => {
                    Main.SetMenu(null);
                })
            )
            .AddChild(
                new Button(UISystem, "Settings").Builder()
                .SetEventListener(EntityEventType.OnClick, entity => {
                    CreateSettingsMenu();
                })
            )
            .AddChild(
                new Button(UISystem, "Exit").Builder()
                .SetEventListener(EntityEventType.OnClick, entity => {
                    Main.ForceExit();
                })
            )
        );
    }

    private void CreateSettingsMenu()
    {
        int padding = 4;

        SetPage(
            new Panel(UISystem) {
                Anchor = Iguina.Defs.Anchor.Center,
                Identifier = "Settings"
            }
            .Builder()
            .SetSizeInPixels(Main.ScreenSize.X, Main.ScreenSize.Y)
            .OverrideStyles(styles => {
                styles.Padding = new(padding, padding, padding, padding);
                styles.TintColor = new(0, 0, 0, 0);
            })
            .AddChild(
                new Panel(UISystem) {
                    Anchor = Iguina.Defs.Anchor.AutoInlineLTR
                }
                .Builder()
                .SetSizeInPercents(2/12f * 100, 100)
                .AddChild(
                    new Button(UISystem, "Back") {
                        Anchor = Iguina.Defs.Anchor.BottomCenter
                    }
                    .Builder()
                    .SetEventListener(EntityEventType.OnClick, entity => {
                        CreatePauseMenu();
                    })
                )
            )
            .AddChild(
                new Panel(UISystem) {
                    Anchor = Iguina.Defs.Anchor.CenterRight
                }
                .Builder()
                .SetSizeInPercents((1 - 2/12f) * 100 * (1 - 4f/Main.ScreenSize.X), 100)
            )
        );
    }
}
